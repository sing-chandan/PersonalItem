using Microsoft.EntityFrameworkCore;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.Modules.Reports.Application.Services;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Infrastructure.Services;

public class PurchaseReportService : IPurchaseReportService
{
    private readonly ApplicationDbContext _context;

    public PurchaseReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PurchaseReportResponse>> GetPurchaseReportAsync(PurchaseReportRequest request, Guid tenantId)
    {
        try
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            // Get purchase orders
            var ordersQuery = _context.PurchaseOrders
                .Include(po => po.Items)
                .Include(po => po.Vendor)
                .Where(po => po.TenantId == tenantId &&
                            po.OrderDate >= startDate &&
                            po.OrderDate <= endDate);

            if (request.VendorId.HasValue)
                ordersQuery = ordersQuery.Where(po => po.VendorId == request.VendorId.Value);

            var orders = await ordersQuery.ToListAsync();

            // Get GRNs
            var grnsQuery = _context.GoodsReceiptNotes
                .Where(g => g.TenantId == tenantId &&
                           g.ReceiptDate >= startDate &&
                           g.ReceiptDate <= endDate);

            var grns = await grnsQuery.ToListAsync();

            // Calculate totals
            var totalPurchases = orders.Sum(o => o.TotalAmount);
            var receivedOrders = orders.Where(o => o.Status == Modules.Purchases.Domain.Entities.PurchaseOrderStatus.FullyReceived ||
                                                    o.Status == Modules.Purchases.Domain.Entities.PurchaseOrderStatus.Closed);
            var totalReceived = receivedOrders.Sum(o => o.TotalAmount);
            var totalPending = totalPurchases - totalReceived;

            // Purchases by period (daily)
            var purchasesByPeriod = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new PurchasesByPeriodItem
                {
                    Date = g.Key,
                    PurchaseAmount = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Top vendors
            var topVendors = orders
                .GroupBy(o => new { o.VendorId, o.Vendor.CompanyName, o.Vendor.Email, o.Vendor.OutstandingBalance })
                .Select(g => new TopVendorItem
                {
                    VendorId = g.Key.VendorId,
                    VendorName = g.Key.CompanyName,
                    Email = g.Key.Email,
                    TotalPurchases = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    OutstandingBalance = g.Key.OutstandingBalance
                })
                .OrderByDescending(x => x.TotalPurchases)
                .Take(10)
                .ToList();

            // Top products
            var topProducts = orders
                .SelectMany(o => o.Items)
                .Where(i => !request.ProductId.HasValue || i.ProductId == request.ProductId.Value)
                .GroupBy(i => new { i.ProductId, i.ProductCode, i.ProductName })
                .Select(g => new TopPurchaseProductItem
                {
                    ProductId = g.Key.ProductId,
                    ProductCode = g.Key.ProductCode,
                    ProductName = g.Key.ProductName,
                    QuantityPurchased = (int)g.Sum(i => i.Quantity),
                    PurchaseAmount = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(x => x.PurchaseAmount)
                .Take(10)
                .ToList();

            var response = new PurchaseReportResponse
            {
                TotalPurchases = totalPurchases,
                TotalPurchaseOrders = orders.Count,
                TotalGRNs = grns.Count,
                TotalReceived = totalReceived,
                TotalPending = totalPending,
                PurchasesByPeriod = purchasesByPeriod,
                TopVendors = topVendors,
                TopProducts = topProducts
            };

            return Result<PurchaseReportResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            return Result<PurchaseReportResponse>.Fail($"Failed to generate purchase report: {ex.Message}");
        }
    }
}
