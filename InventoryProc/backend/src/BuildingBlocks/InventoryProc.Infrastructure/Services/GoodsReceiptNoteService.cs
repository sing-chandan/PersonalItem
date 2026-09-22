using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.Modules.Purchases.Application.Services;
using InventoryProc.Modules.Purchases.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.Infrastructure.Services;

public class GoodsReceiptNoteService : IGoodsReceiptNoteService
{
    private readonly ApplicationDbContext _context;

    public GoodsReceiptNoteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GoodsReceiptNoteResponse>>> GetAllAsync(Guid tenantId)
    {
        var grns = await _context.GoodsReceiptNotes
            .Include(grn => grn.Items)
            .Where(grn => grn.TenantId == tenantId && grn.IsActive)
            .OrderByDescending(grn => grn.CreatedAt)
            .ToListAsync();

        var response = grns.Select(MapToResponse).ToList();
        return Result<List<GoodsReceiptNoteResponse>>.Ok(response);
    }

    public async Task<Result<GoodsReceiptNoteResponse>> GetByIdAsync(Guid id, Guid tenantId)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(grn => grn.Items)
            .FirstOrDefaultAsync(grn => grn.Id == id && grn.TenantId == tenantId && grn.IsActive);

        if (grn == null)
            return Result<GoodsReceiptNoteResponse>.Fail("GRN not found");

        return Result<GoodsReceiptNoteResponse>.Ok(MapToResponse(grn));
    }

    public async Task<Result<GoodsReceiptNoteResponse>> GetByGRNNumberAsync(string grnNumber, Guid tenantId)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(grn => grn.Items)
            .FirstOrDefaultAsync(grn => grn.GRNNumber == grnNumber && grn.TenantId == tenantId && grn.IsActive);

        if (grn == null)
            return Result<GoodsReceiptNoteResponse>.Fail("GRN not found");

        return Result<GoodsReceiptNoteResponse>.Ok(MapToResponse(grn));
    }

    public async Task<Result<List<GoodsReceiptNoteResponse>>> GetByPurchaseOrderAsync(Guid purchaseOrderId, Guid tenantId)
    {
        var grns = await _context.GoodsReceiptNotes
            .Include(grn => grn.Items)
            .Where(grn => grn.PurchaseOrderId == purchaseOrderId && grn.TenantId == tenantId && grn.IsActive)
            .OrderByDescending(grn => grn.CreatedAt)
            .ToListAsync();

        var response = grns.Select(MapToResponse).ToList();
        return Result<List<GoodsReceiptNoteResponse>>.Ok(response);
    }

    public async Task<Result<GoodsReceiptNoteResponse>> CreateAsync(CreateGoodsReceiptNoteRequest request, Guid tenantId, Guid userId)
    {
        // Get purchase order with items
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId && po.TenantId == tenantId);

        if (purchaseOrder == null)
            return Result<GoodsReceiptNoteResponse>.Fail("Purchase order not found");

        if (purchaseOrder.Status != PurchaseOrderStatus.Approved &&
            purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
            return Result<GoodsReceiptNoteResponse>.Fail("Purchase order must be approved before receiving goods");

        // Check if GRN number already exists
        var existingGrn = await _context.GoodsReceiptNotes
            .FirstOrDefaultAsync(grn => grn.GRNNumber == request.GRNNumber && grn.TenantId == tenantId);

        if (existingGrn != null)
            return Result<GoodsReceiptNoteResponse>.Fail("GRN number already exists");

        var grn = new GoodsReceiptNote(
            tenantId,
            request.GRNNumber,
            request.ReceiptDate,
            purchaseOrder.Id,
            purchaseOrder.OrderNumber,
            purchaseOrder.VendorId,
            purchaseOrder.VendorName)
        {
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            ReceivedBy = request.ReceivedBy,
            Notes = request.Notes,
            CreatedBy = userId
        };

        // Add items
        foreach (var itemRequest in request.Items)
        {
            var poItem = purchaseOrder.Items.FirstOrDefault(i => i.Id == itemRequest.PurchaseOrderItemId);
            if (poItem == null)
                return Result<GoodsReceiptNoteResponse>.Fail($"Purchase order item {itemRequest.PurchaseOrderItemId} not found");

            var item = new GoodsReceiptNoteItem(
                itemRequest.PurchaseOrderItemId,
                itemRequest.ProductId,
                itemRequest.ProductName,
                itemRequest.ProductCode,
                itemRequest.OrderedQuantity,
                itemRequest.ReceivedQuantity,
                itemRequest.Unit);

            if (itemRequest.RejectedQuantity > 0)
            {
                item.RecordQualityCheck(itemRequest.AcceptedQuantity, itemRequest.RejectedQuantity, itemRequest.RejectionReason);
            }

            grn.AddItem(item);
        }

        _context.GoodsReceiptNotes.Add(grn);
        await _context.SaveChangesAsync();

        return Result<GoodsReceiptNoteResponse>.Ok(MapToResponse(grn), "GRN created successfully");
    }

    public async Task<Result<GoodsReceiptNoteResponse>> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(grn => grn.Items)
            .FirstOrDefaultAsync(grn => grn.Id == id && grn.TenantId == tenantId && grn.IsActive);

        if (grn == null)
            return Result<GoodsReceiptNoteResponse>.Fail("GRN not found");

        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == grn.PurchaseOrderId && po.TenantId == tenantId);

        if (purchaseOrder == null)
            return Result<GoodsReceiptNoteResponse>.Fail("Purchase order not found");

        try
        {
            grn.Complete();
            grn.UpdatedBy = userId;

            // Update purchase order item received quantities
            foreach (var grnItem in grn.Items)
            {
                var poItem = purchaseOrder.Items.FirstOrDefault(i => i.Id == grnItem.PurchaseOrderItemId);
                if (poItem != null)
                {
                    poItem.RecordReceivedQuantity(grnItem.AcceptedQuantity);
                }

                // Update product stock
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == grnItem.ProductId && p.TenantId == tenantId);

                if (product != null)
                {
                    product.UpdateStock(grnItem.AcceptedQuantity);
                }
            }

            // Update purchase order status based on received quantities
            var allItemsFullyReceived = purchaseOrder.Items.All(i => i.IsFullyReceived());
            var anyItemPartiallyReceived = purchaseOrder.Items.Any(i => i.ReceivedQuantity > 0 && !i.IsFullyReceived());

            if (allItemsFullyReceived)
            {
                purchaseOrder.MarkAsFullyReceived();
            }
            else if (anyItemPartiallyReceived || purchaseOrder.Items.Any(i => i.IsFullyReceived()))
            {
                purchaseOrder.MarkAsPartiallyReceived();
            }

            purchaseOrder.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Result<GoodsReceiptNoteResponse>.Ok(MapToResponse(grn), "GRN completed and stock updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<GoodsReceiptNoteResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<GoodsReceiptNoteResponse>> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(grn => grn.Items)
            .FirstOrDefaultAsync(grn => grn.Id == id && grn.TenantId == tenantId && grn.IsActive);

        if (grn == null)
            return Result<GoodsReceiptNoteResponse>.Fail("GRN not found");

        try
        {
            grn.Cancel();
            grn.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return Result<GoodsReceiptNoteResponse>.Ok(MapToResponse(grn), "GRN cancelled successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<GoodsReceiptNoteResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, Guid tenantId)
    {
        var grn = await _context.GoodsReceiptNotes
            .FirstOrDefaultAsync(grn => grn.Id == id && grn.TenantId == tenantId && grn.IsActive);

        if (grn == null)
            return Result<bool>.Fail("GRN not found");

        if (grn.Status == GRNStatus.Completed)
            return Result<bool>.Fail("Cannot delete completed GRN");

        _context.GoodsReceiptNotes.Remove(grn);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true, "GRN deleted successfully");
    }

    private static GoodsReceiptNoteResponse MapToResponse(GoodsReceiptNote grn)
    {
        return new GoodsReceiptNoteResponse
        {
            Id = grn.Id,
            TenantId = grn.TenantId,
            GRNNumber = grn.GRNNumber,
            ReceiptDate = grn.ReceiptDate,
            PurchaseOrderId = grn.PurchaseOrderId,
            PurchaseOrderNumber = grn.PurchaseOrderNumber,
            VendorId = grn.VendorId,
            VendorName = grn.VendorName,
            InvoiceNumber = grn.InvoiceNumber,
            InvoiceDate = grn.InvoiceDate,
            ReceivedBy = grn.ReceivedBy,
            Notes = grn.Notes,
            Status = grn.Status,
            IsActive = grn.IsActive,
            CreatedAt = grn.CreatedAt,
            UpdatedAt = grn.UpdatedAt,
            Items = grn.Items.Select(i => new GoodsReceiptNoteItemResponse
            {
                Id = i.Id,
                GoodsReceiptNoteId = i.GoodsReceiptNoteId,
                PurchaseOrderItemId = i.PurchaseOrderItemId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductCode = i.ProductCode,
                OrderedQuantity = i.OrderedQuantity,
                ReceivedQuantity = i.ReceivedQuantity,
                AcceptedQuantity = i.AcceptedQuantity,
                RejectedQuantity = i.RejectedQuantity,
                Unit = i.Unit,
                Notes = i.Notes,
                RejectionReason = i.RejectionReason
            }).ToList()
        };
    }
}
