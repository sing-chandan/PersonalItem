using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.Modules.Sales.Application.Services;
using InventoryProc.Modules.Sales.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.Infrastructure.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly ApplicationDbContext _context;

    public SalesOrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SalesOrderResponse>>> GetAllAsync(Guid tenantId)
    {
        var orders = await _context.SalesOrders
            .Include(so => so.Items)
            .Where(so => so.TenantId == tenantId && so.IsActive)
            .OrderByDescending(so => so.OrderDate)
            .ToListAsync();

        var response = orders.Select(MapToResponse).ToList();
        return Result<List<SalesOrderResponse>>.Ok(response);
    }

    public async Task<Result<SalesOrderResponse>> GetByIdAsync(Guid id, Guid tenantId)
    {
        var order = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (order == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        return Result<SalesOrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<SalesOrderResponse>> GetByOrderNumberAsync(string orderNumber, Guid tenantId)
    {
        var order = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.OrderNumber == orderNumber && so.TenantId == tenantId && so.IsActive);

        if (order == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        return Result<SalesOrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<Result<SalesOrderResponse>> CreateAsync(CreateSalesOrderRequest request, Guid tenantId, Guid userId)
    {
        // Check for duplicate order number
        var exists = await _context.SalesOrders
            .AnyAsync(so => so.OrderNumber == request.OrderNumber && so.TenantId == tenantId);

        if (exists)
            return Result<SalesOrderResponse>.Fail("Order number already exists");

        // Verify customer exists
        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == request.CustomerId && c.TenantId == tenantId && c.IsActive);

        if (!customerExists)
            return Result<SalesOrderResponse>.Fail("Customer not found");

        try
        {
            var salesOrder = new SalesOrder(
                tenantId,
                request.OrderNumber,
                request.CustomerId,
                request.CustomerName,
                request.OrderDate);

            salesOrder.Notes = request.Notes;
            salesOrder.ShippingAddress = request.ShippingAddress;
            salesOrder.BillingAddress = request.BillingAddress;
            salesOrder.CreatedBy = userId;

            // Add items
            foreach (var itemRequest in request.Items)
            {
                var item = new SalesOrderItem(
                    itemRequest.ProductId,
                    itemRequest.ProductName,
                    itemRequest.ProductCode,
                    itemRequest.Quantity,
                    itemRequest.Unit,
                    itemRequest.UnitPrice,
                    itemRequest.TaxRate);

                item.Notes = itemRequest.Notes;
                salesOrder.AddItem(item);
            }

            _context.SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync();

            return Result<SalesOrderResponse>.Ok(MapToResponse(salesOrder), "Sales order created successfully");
        }
        catch (Exception ex)
        {
            return Result<SalesOrderResponse>.Fail($"Error creating sales order: {ex.Message}");
        }
    }

    public async Task<Result<SalesOrderResponse>> UpdateAsync(Guid id, UpdateSalesOrderRequest request, Guid tenantId, Guid userId)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        if (salesOrder.Status != SalesOrderStatus.Draft)
            return Result<SalesOrderResponse>.Fail("Only draft orders can be updated");

        try
        {
            salesOrder.OrderDate = request.OrderDate;
            salesOrder.Notes = request.Notes;
            salesOrder.ShippingAddress = request.ShippingAddress;
            salesOrder.BillingAddress = request.BillingAddress;
            salesOrder.UpdatedBy = userId;

            // Remove items not in the update request
            var itemIdsToKeep = request.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToList();
            var itemsToRemove = salesOrder.Items.Where(i => !itemIdsToKeep.Contains(i.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                salesOrder.RemoveItem(item.Id);
            }

            // Update or add items
            foreach (var itemRequest in request.Items)
            {
                if (itemRequest.Id.HasValue)
                {
                    // Update existing item
                    var existingItem = salesOrder.Items.FirstOrDefault(i => i.Id == itemRequest.Id.Value);
                    if (existingItem != null)
                    {
                        existingItem.ProductId = itemRequest.ProductId;
                        existingItem.ProductName = itemRequest.ProductName;
                        existingItem.ProductCode = itemRequest.ProductCode;
                        existingItem.Unit = itemRequest.Unit;
                        existingItem.TaxRate = itemRequest.TaxRate;
                        existingItem.Notes = itemRequest.Notes;
                        existingItem.UpdateQuantity(itemRequest.Quantity);
                        existingItem.UpdatePrice(itemRequest.UnitPrice);
                    }
                }
                else
                {
                    // Add new item
                    var newItem = new SalesOrderItem(
                        itemRequest.ProductId,
                        itemRequest.ProductName,
                        itemRequest.ProductCode,
                        itemRequest.Quantity,
                        itemRequest.Unit,
                        itemRequest.UnitPrice,
                        itemRequest.TaxRate);

                    newItem.Notes = itemRequest.Notes;
                    salesOrder.AddItem(newItem);
                }
            }

            salesOrder.RecalculateTotals();

            await _context.SaveChangesAsync();

            return Result<SalesOrderResponse>.Ok(MapToResponse(salesOrder), "Sales order updated successfully");
        }
        catch (Exception ex)
        {
            return Result<SalesOrderResponse>.Fail($"Error updating sales order: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, Guid tenantId)
    {
        var salesOrder = await _context.SalesOrders
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result.Fail("Sales order not found");

        if (salesOrder.Status != SalesOrderStatus.Draft)
            return Result.Fail("Only draft orders can be deleted");

        salesOrder.IsActive = false;
        salesOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Ok("Sales order deleted successfully");
    }

    public async Task<Result<SalesOrderResponse>> ConfirmAsync(Guid id, Guid tenantId, Guid userId)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        try
        {
            salesOrder.Confirm();
            salesOrder.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Result<SalesOrderResponse>.Ok(MapToResponse(salesOrder), "Sales order confirmed successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<SalesOrderResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<SalesOrderResponse>> ShipAsync(Guid id, Guid tenantId, Guid userId, DateTime? shippedDate = null)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        try
        {
            salesOrder.Ship(shippedDate);
            salesOrder.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Result<SalesOrderResponse>.Ok(MapToResponse(salesOrder), "Sales order shipped successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<SalesOrderResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<SalesOrderResponse>> DeliverAsync(Guid id, Guid tenantId, Guid userId, DateTime? deliveredDate = null)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        try
        {
            salesOrder.Deliver(deliveredDate);
            salesOrder.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Result<SalesOrderResponse>.Ok(MapToResponse(salesOrder), "Sales order delivered successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<SalesOrderResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<SalesOrderResponse>> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        try
        {
            salesOrder.Cancel();
            salesOrder.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Result<SalesOrderResponse>.Ok(MapToResponse(salesOrder), "Sales order cancelled successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Result<SalesOrderResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<SalesOrderResponse>> ApplyDiscountAsync(Guid id, decimal discountAmount, Guid tenantId, Guid userId)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && so.TenantId == tenantId && so.IsActive);

        if (salesOrder == null)
            return Result<SalesOrderResponse>.Fail("Sales order not found");

        if (salesOrder.Status != SalesOrderStatus.Draft)
            return Result<SalesOrderResponse>.Fail("Can only apply discount to draft orders");

        try
        {
            salesOrder.ApplyDiscount(discountAmount);
            salesOrder.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return Result<SalesOrderResponse>.Ok(MapToResponse(salesOrder), "Discount applied successfully");
        }
        catch (Exception ex)
        {
            return Result<SalesOrderResponse>.Fail($"Error applying discount: {ex.Message}");
        }
    }

    public async Task<Result<List<SalesOrderResponse>>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        var orders = await _context.SalesOrders
            .Include(so => so.Items)
            .Where(so => so.CustomerId == customerId && so.TenantId == tenantId && so.IsActive)
            .OrderByDescending(so => so.OrderDate)
            .ToListAsync();

        var response = orders.Select(MapToResponse).ToList();
        return Result<List<SalesOrderResponse>>.Ok(response);
    }

    private static SalesOrderResponse MapToResponse(SalesOrder order)
    {
        return new SalesOrderResponse
        {
            Id = order.Id,
            TenantId = order.TenantId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            ShippedDate = order.ShippedDate,
            DeliveredDate = order.DeliveredDate,
            IsActive = order.IsActive,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(item => new SalesOrderItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductCode = item.ProductCode,
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                Notes = item.Notes
            }).ToList()
        };
    }
}
