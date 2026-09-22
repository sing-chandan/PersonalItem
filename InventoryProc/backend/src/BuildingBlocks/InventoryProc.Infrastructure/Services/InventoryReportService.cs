using Microsoft.EntityFrameworkCore;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.Modules.Reports.Application.Services;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Infrastructure.Services;

public class InventoryReportService : IInventoryReportService
{
    private readonly ApplicationDbContext _context;

    public InventoryReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InventoryReportResponse>> GetInventoryReportAsync(InventoryReportRequest request, Guid tenantId)
    {
        try
        {
            // Get products with category info
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId && p.IsActive);

            if (request.CategoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == request.CategoryId.Value);

            if (request.BrandId.HasValue)
                productsQuery = productsQuery.Where(p => p.BrandId == request.BrandId.Value);

            var products = await productsQuery.ToListAsync();

            // Calculate totals
            var totalProducts = products.Count;
            var lowStockProducts = products.Count(p => p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel.Value && p.CurrentStock > 0);
            var outOfStockProducts = products.Count(p => p.CurrentStock == 0);
            var totalStockValue = products.Sum(p => p.CurrentStock * p.SalePrice);

            // Stock levels
            var stockLevels = products
                .Select(p => new StockLevelItem
                {
                    ProductId = p.Id,
                    ProductCode = p.Code,
                    ProductName = p.Name,
                    CategoryName = p.Category?.Name ?? "N/A",
                    CurrentStock = (int)p.CurrentStock,
                    MinimumStock = (int)(p.MinStockLevel ?? 0),
                    UnitPrice = p.SalePrice,
                    StockValue = p.CurrentStock * p.SalePrice
                })
                .OrderBy(x => x.ProductName)
                .ToList();

            // Low stock items
            var lowStockItems = products
                .Where(p => p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel.Value)
                .Select(p => new LowStockItem
                {
                    ProductId = p.Id,
                    ProductCode = p.Code,
                    ProductName = p.Name,
                    CurrentStock = (int)p.CurrentStock,
                    MinimumStock = (int)(p.MinStockLevel ?? 0),
                    ShortageQuantity = (int)(p.MinStockLevel ?? 0) - (int)p.CurrentStock
                })
                .OrderBy(x => x.CurrentStock)
                .ToList();

            // Recent movements (from sales and purchases in last 30 days)
            var recentMovements = new List<StockMovementItem>();

            var recentSales = await _context.SalesOrderItems
                .Include(i => i.SalesOrder)
                .Where(i => i.SalesOrder.TenantId == tenantId &&
                           i.SalesOrder.OrderDate >= DateTime.UtcNow.AddDays(-30))
                .OrderByDescending(i => i.SalesOrder.OrderDate)
                .Take(20)
                .ToListAsync();

            recentMovements.AddRange(recentSales.Select(i => new StockMovementItem
            {
                Date = i.SalesOrder.OrderDate,
                ProductCode = i.ProductCode,
                ProductName = i.ProductName,
                MovementType = "Sale",
                Quantity = -(int)i.Quantity,
                Reference = i.SalesOrder.OrderNumber
            }));

            var recentGRNs = await _context.GoodsReceiptNoteItems
                .Include(i => i.GoodsReceiptNote)
                .Where(i => i.GoodsReceiptNote.TenantId == tenantId &&
                           i.GoodsReceiptNote.ReceiptDate >= DateTime.UtcNow.AddDays(-30) &&
                           i.GoodsReceiptNote.Status == Modules.Purchases.Domain.Entities.GRNStatus.Completed)
                .OrderByDescending(i => i.GoodsReceiptNote.ReceiptDate)
                .Take(20)
                .ToListAsync();

            recentMovements.AddRange(recentGRNs.Select(i => new StockMovementItem
            {
                Date = i.GoodsReceiptNote.ReceiptDate,
                ProductCode = i.ProductCode,
                ProductName = i.ProductName,
                MovementType = "Purchase",
                Quantity = (int)i.AcceptedQuantity,
                Reference = i.GoodsReceiptNote.GRNNumber
            }));

            recentMovements = recentMovements.OrderByDescending(m => m.Date).Take(30).ToList();

            var response = new InventoryReportResponse
            {
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                TotalStockValue = totalStockValue,
                StockLevels = request.LowStockOnly == true ? lowStockItems.Select(l => new StockLevelItem
                {
                    ProductId = l.ProductId,
                    ProductCode = l.ProductCode,
                    ProductName = l.ProductName,
                    CategoryName = "",
                    CurrentStock = l.CurrentStock,
                    MinimumStock = l.MinimumStock,
                    UnitPrice = 0,
                    StockValue = 0
                }).ToList() : stockLevels,
                LowStockItems = lowStockItems,
                RecentMovements = recentMovements
            };

            return Result<InventoryReportResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            return Result<InventoryReportResponse>.Fail($"Failed to generate inventory report: {ex.Message}");
        }
    }
}
