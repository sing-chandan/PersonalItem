using Microsoft.EntityFrameworkCore;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.Modules.Reports.Application.Services;
using InventoryProc.SharedKernel.Common;
using InventoryProc.Modules.Sales.Domain.Entities;

namespace InventoryProc.Infrastructure.Services;

public class SalesReportService : ISalesReportService
{
    private readonly ApplicationDbContext _context;

    public SalesReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SalesReportResponse>> GetSalesReportAsync(SalesReportRequest request, Guid tenantId)
    {
        try
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            // Get sales orders
            var ordersQuery = _context.SalesOrders
                .Include(so => so.Items)
                .Include(so => so.Customer)
                .Where(so => so.TenantId == tenantId &&
                            so.OrderDate >= startDate &&
                            so.OrderDate <= endDate);

            if (request.CustomerId.HasValue)
                ordersQuery = ordersQuery.Where(so => so.CustomerId == request.CustomerId.Value);

            var orders = await ordersQuery.ToListAsync();

            // Get invoices
            var invoicesQuery = _context.Invoices
                .Where(i => i.TenantId == tenantId &&
                           i.InvoiceDate >= startDate &&
                           i.InvoiceDate <= endDate);

            var invoices = await invoicesQuery.ToListAsync();

            // Calculate totals
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var totalPaid = invoices.Sum(i => i.PaidAmount);
            var totalOutstanding = invoices.Sum(i => i.BalanceAmount);

            // Sales by period (daily)
            var salesByPeriod = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesByPeriodItem
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Top customers
            var topCustomers = orders
                .GroupBy(o => new { o.CustomerId, o.Customer.CompanyName, o.Customer.Email })
                .Select(g => new TopCustomerItem
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.CompanyName,
                    Email = g.Key.Email,
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToList();

            // Top products
            var topProducts = orders
                .SelectMany(o => o.Items)
                .Where(i => !request.ProductId.HasValue || i.ProductId == request.ProductId.Value)
                .GroupBy(i => new { i.ProductId, i.ProductCode, i.ProductName })
                .Select(g => new TopProductItem
                {
                    ProductId = g.Key.ProductId,
                    ProductCode = g.Key.ProductCode,
                    ProductName = g.Key.ProductName,
                    QuantitySold = (int)g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            var response = new SalesReportResponse
            {
                TotalRevenue = totalRevenue,
                TotalOrders = orders.Count,
                TotalInvoices = invoices.Count,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,
                SalesByPeriod = salesByPeriod,
                TopCustomers = topCustomers,
                TopProducts = topProducts
            };

            return Result<SalesReportResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            return Result<SalesReportResponse>.Fail($"Failed to generate sales report: {ex.Message}");
        }
    }
}
