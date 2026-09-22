using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.Modules.Purchases.Application.Services;
using InventoryProc.Modules.Purchases.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.Infrastructure.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly ApplicationDbContext _context;

    public PurchaseOrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PurchaseOrderResponse>>> GetAllAsync(Guid tenantId)
    {
        var orders = await _context.PurchaseOrders
            .Include(po => po.Items)
            .Where(po => po.TenantId == tenantId && po.IsActive)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync();

        var response = orders.Select(MapToResponse).ToList();
        return Result<List<PurchaseOrderResponse>>.Ok(response);
    }

    public async Task<Result<PurchaseOrderResponse>> GetByIdAsync(Guid id, Guid tenantId)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order not found");

        return Result<PurchaseOrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<PurchaseOrderResponse>> GetByOrderNumberAsync(string orderNumber, Guid tenantId)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order not found");

        return Result<PurchaseOrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<List<PurchaseOrderResponse>>> GetByVendorAsync(Guid vendorId, Guid tenantId)
    {
        var orders = await _context.PurchaseOrders
            .Include(po => po.Items)
            .Where(po => po.VendorId == vendorId && po.TenantId == tenantId && po.IsActive)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync();

        var response = orders.Select(MapToResponse).ToList();
        return Result<List<PurchaseOrderResponse>>.Ok(response);
    }

    public async Task<Result<PurchaseOrderResponse>> CreateAsync(CreatePurchaseOrderRequest request, Guid tenantId, Guid userId)
    {
        // Check if order number already exists
        var existingOrder = await _context.PurchaseOrders
            .FirstOrDefaultAsync(po => po.OrderNumber == request.OrderNumber && po.TenantId == tenantId);

        if (existingOrder != null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order number already exists");

        var order = new PurchaseOrder(
            tenantId,
            request.OrderNumber,
            request.OrderDate,
            request.VendorId,
            request.VendorName)
        {
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes,
            DeliveryAddress = request.DeliveryAddress,
            BillingAddress = request.BillingAddress,
            CreatedBy = userId
        };

        // Add items
        foreach (var itemRequest in request.Items)
        {
            var item = new PurchaseOrderItem(
                itemRequest.ProductId,
                itemRequest.ProductName,
                itemRequest.ProductCode,
                itemRequest.Quantity,
                itemRequest.Unit,
                itemRequest.UnitPrice,
                itemRequest.TaxRate)
            {
                Notes = itemRequest.Notes
            };

            order.AddItem(item);
        }

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        return Result<PurchaseOrderResponse>.Ok(MapToResponse(order), "Purchase order created successfully");
    }

    public async Task<Result<PurchaseOrderResponse>> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, Guid tenantId, Guid userId)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order not found");

        if (order.Status != PurchaseOrderStatus.Draft)
            return Result<PurchaseOrderResponse>.Fail("Only draft purchase orders can be updated");

        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        order.Notes = request.Notes;
        order.DeliveryAddress = request.DeliveryAddress;
        order.BillingAddress = request.BillingAddress;
        order.UpdatedBy = userId;

        // Remove all existing items
        var existingItems = order.Items.ToList();
        foreach (var item in existingItems)
        {
            order.RemoveItem(item.Id);
        }

        // Add new items
        foreach (var itemRequest in request.Items)
        {
            var item = new PurchaseOrderItem(
                itemRequest.ProductId,
                itemRequest.ProductName,
                itemRequest.ProductCode,
                itemRequest.Quantity,
                itemRequest.Unit,
                itemRequest.UnitPrice,
                itemRequest.TaxRate)
            {
                Notes = itemRequest.Notes
            };

            order.AddItem(item);
        }

        await _context.SaveChangesAsync();

        return Result<PurchaseOrderResponse>.Ok(MapToResponse(order), "Purchase order updated successfully");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, Guid tenantId)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<bool>.Fail("Purchase order not found");

        if (order.Status != PurchaseOrderStatus.Draft && order.Status != PurchaseOrderStatus.Cancelled)
            return Result<bool>.Fail("Only draft or cancelled purchase orders can be deleted");

        _context.PurchaseOrders.Remove(order);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true, "Purchase order deleted successfully");
    }

    public async Task<Result<PurchaseOrderResponse>> ApproveAsync(Guid id, Guid tenantId, Guid userId, string approvedBy)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order not found");

        try
        {
            order.Approve(approvedBy);
            order.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return Result<PurchaseOrderResponse>.Ok(MapToResponse(order), "Purchase order approved successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<PurchaseOrderResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderResponse>> CloseAsync(Guid id, Guid tenantId, Guid userId)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order not found");

        try
        {
            order.Close();
            order.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return Result<PurchaseOrderResponse>.Ok(MapToResponse(order), "Purchase order closed successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<PurchaseOrderResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderResponse>> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order not found");

        try
        {
            order.Cancel();
            order.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return Result<PurchaseOrderResponse>.Ok(MapToResponse(order), "Purchase order cancelled successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<PurchaseOrderResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderResponse>> ApplyDiscountAsync(Guid id, decimal discountAmount, Guid tenantId, Guid userId)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId && po.IsActive);

        if (order == null)
            return Result<PurchaseOrderResponse>.Fail("Purchase order not found");

        try
        {
            order.ApplyDiscount(discountAmount);
            order.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return Result<PurchaseOrderResponse>.Ok(MapToResponse(order), "Discount applied successfully");
        }
        catch (ArgumentException ex)
        {
            return Result<PurchaseOrderResponse>.Fail(ex.Message);
        }
    }

    private static PurchaseOrderResponse MapToResponse(PurchaseOrder order)
    {
        return new PurchaseOrderResponse
        {
            Id = order.Id,
            TenantId = order.TenantId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            VendorId = order.VendorId,
            VendorName = order.VendorName,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            Status = order.Status,
            DeliveryAddress = order.DeliveryAddress,
            BillingAddress = order.BillingAddress,
            ApprovedBy = order.ApprovedBy,
            ApprovedDate = order.ApprovedDate,
            IsActive = order.IsActive,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new PurchaseOrderItemResponse
            {
                Id = i.Id,
                PurchaseOrderId = i.PurchaseOrderId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductCode = i.ProductCode,
                Quantity = i.Quantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                TaxRate = i.TaxRate,
                TaxAmount = i.TaxAmount,
                ReceivedQuantity = i.ReceivedQuantity,
                Notes = i.Notes
            }).ToList()
        };
    }
}
