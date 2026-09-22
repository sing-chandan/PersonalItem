using Microsoft.EntityFrameworkCore;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.Modules.Reports.Application.Services;
using InventoryProc.SharedKernel.Common;
using InventoryProc.Modules.Sales.Domain.Entities;
using InventoryProc.Modules.Purchases.Domain.Entities;

namespace InventoryProc.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardSummaryResponse>> GetDashboardSummaryAsync(Guid tenantId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            // Sales Summary
            var salesOrders = await _context.SalesOrders
                .Where(so => so.TenantId == tenantId)
                .ToListAsync();

            var invoices = await _context.Invoices
                .Where(i => i.TenantId == tenantId)
                .ToListAsync();

            var salesSummary = new SalesSummary
            {
                TodayRevenue = salesOrders.Where(so => so.OrderDate.Date == today).Sum(so => so.TotalAmount),
                MonthRevenue = salesOrders.Where(so => so.OrderDate >= monthStart).Sum(so => so.TotalAmount),
                YearRevenue = salesOrders.Where(so => so.OrderDate >= yearStart).Sum(so => so.TotalAmount),
                TodayOrders = salesOrders.Count(so => so.OrderDate.Date == today),
                MonthOrders = salesOrders.Count(so => so.OrderDate >= monthStart),
                PendingInvoices = invoices.Count(i => i.PaymentStatus == InvoicePaymentStatus.Unpaid || i.PaymentStatus == InvoicePaymentStatus.PartiallyPaid),
                OutstandingAmount = invoices.Where(i => i.PaymentStatus != InvoicePaymentStatus.Paid).Sum(i => i.BalanceAmount)
            };

            // Purchase Summary
            var purchaseOrders = await _context.PurchaseOrders
                .Where(po => po.TenantId == tenantId)
                .ToListAsync();

            var grns = await _context.GoodsReceiptNotes
                .Where(g => g.TenantId == tenantId)
                .ToListAsync();

            var purchaseSummary = new PurchaseSummary
            {
                TodayPurchases = purchaseOrders.Where(po => po.OrderDate.Date == today).Sum(po => po.TotalAmount),
                MonthPurchases = purchaseOrders.Where(po => po.OrderDate >= monthStart).Sum(po => po.TotalAmount),
                YearPurchases = purchaseOrders.Where(po => po.OrderDate >= yearStart).Sum(po => po.TotalAmount),
                TodayPOs = purchaseOrders.Count(po => po.OrderDate.Date == today),
                MonthPOs = purchaseOrders.Count(po => po.OrderDate >= monthStart),
                PendingPOs = purchaseOrders.Count(po => po.Status == PurchaseOrderStatus.Approved ||
                                                        po.Status == PurchaseOrderStatus.PartiallyReceived),
                PendingGRNs = grns.Count(g => g.Status == GRNStatus.Draft)
            };

            // Inventory Summary
            var products = await _context.Products
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .ToListAsync();

            var inventorySummary = new InventorySummary
            {
                TotalProducts = products.Count,
                LowStockProducts = products.Count(p => p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel.Value && p.CurrentStock > 0),
                OutOfStockProducts = products.Count(p => p.CurrentStock == 0),
                TotalStockValue = products.Sum(p => p.CurrentStock * p.SalePrice)
            };

            // Financial Summary
            var totalRevenue = salesSummary.YearRevenue;
            var totalExpenses = purchaseSummary.YearPurchases;
            var netProfit = totalRevenue - totalExpenses;
            var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

            var financialSummary = new FinancialSummary
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                ProfitMargin = profitMargin,
                AccountsReceivable = salesSummary.OutstandingAmount,
                AccountsPayable = await _context.Vendors.Where(v => v.TenantId == tenantId).SumAsync(v => v.OutstandingBalance)
            };

            // Recent Activities
            var recentActivities = new List<RecentActivityItem>();

            // Recent sales orders
            var recentSalesOrders = salesOrders
                .OrderByDescending(so => so.OrderDate)
                .Take(5)
                .Select(so => new RecentActivityItem
                {
                    Timestamp = so.OrderDate,
                    ActivityType = "Sales Order",
                    Description = $"Sales Order created",
                    Reference = so.OrderNumber,
                    Amount = so.TotalAmount
                });

            recentActivities.AddRange(recentSalesOrders);

            // Recent purchase orders
            var recentPurchaseOrders = purchaseOrders
                .OrderByDescending(po => po.OrderDate)
                .Take(5)
                .Select(po => new RecentActivityItem
                {
                    Timestamp = po.OrderDate,
                    ActivityType = "Purchase Order",
                    Description = $"Purchase Order created",
                    Reference = po.OrderNumber,
                    Amount = po.TotalAmount
                });

            recentActivities.AddRange(recentPurchaseOrders);

            // Recent invoices
            var recentInvoices = invoices
                .OrderByDescending(i => i.InvoiceDate)
                .Take(5)
                .Select(i => new RecentActivityItem
                {
                    Timestamp = i.InvoiceDate,
                    ActivityType = "Invoice",
                    Description = $"Invoice {i.PaymentStatus}",
                    Reference = i.InvoiceNumber,
                    Amount = i.TotalAmount
                });

            recentActivities.AddRange(recentInvoices);

            recentActivities = recentActivities.OrderByDescending(a => a.Timestamp).Take(10).ToList();

            // Top Customers (by sales amount)
            var topCustomers = await _context.SalesOrders
                .Where(so => so.TenantId == tenantId)
                .Include(so => so.Customer)
                .GroupBy(so => new { so.CustomerId, so.Customer!.CompanyName, so.Customer.Email })
                .Select(g => new TopCustomerItem
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.CompanyName,
                    Email = g.Key.Email,
                    TotalRevenue = g.Sum(so => so.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(tc => tc.TotalRevenue)
                .Take(5)
                .ToListAsync();

            // Top Products (by quantity sold)
            var topProducts = await _context.SalesOrderItems
                .Include(soi => soi.SalesOrder)
                .Where(soi => soi.SalesOrder!.TenantId == tenantId)
                .GroupBy(soi => new { soi.ProductId, soi.ProductCode, soi.ProductName })
                .Select(g => new TopProductItem
                {
                    ProductId = g.Key.ProductId,
                    ProductCode = g.Key.ProductCode,
                    ProductName = g.Key.ProductName,
                    QuantitySold = (int)g.Sum(soi => soi.Quantity),
                    Revenue = g.Sum(soi => soi.TotalPrice + soi.TaxAmount)
                })
                .OrderByDescending(tp => tp.QuantitySold)
                .Take(5)
                .ToListAsync();

            // Monthly Trend (last 6 months)
            var monthlyTrend = new List<MonthlyTrendItem>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                var monthStartDate = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

                var monthlySales = salesOrders
                    .Where(so => so.OrderDate >= monthStartDate && so.OrderDate <= monthEndDate)
                    .Sum(so => so.TotalAmount);

                var monthlyPurchases = purchaseOrders
                    .Where(po => po.OrderDate >= monthStartDate && po.OrderDate <= monthEndDate)
                    .Sum(po => po.TotalAmount);

                monthlyTrend.Add(new MonthlyTrendItem
                {
                    Month = monthStartDate.ToString("MMM yyyy"),
                    Sales = monthlySales,
                    Purchases = monthlyPurchases,
                    Profit = monthlySales - monthlyPurchases
                });
            }

            var response = new DashboardSummaryResponse
            {
                Sales = salesSummary,
                Purchases = purchaseSummary,
                Inventory = inventorySummary,
                Financial = financialSummary,
                RecentActivities = recentActivities,
                TopCustomers = topCustomers,
                TopProducts = topProducts,
                MonthlyTrend = monthlyTrend
            };

            return Result<DashboardSummaryResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            return Result<DashboardSummaryResponse>.Fail($"Failed to generate dashboard summary: {ex.Message}");
        }
    }
}
