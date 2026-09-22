namespace InventoryProc.Modules.Reports.Application.DTOs;

public class DashboardSummaryResponse
{
    public SalesSummary Sales { get; set; } = new();
    public PurchaseSummary Purchases { get; set; } = new();
    public InventorySummary Inventory { get; set; } = new();
    public FinancialSummary Financial { get; set; } = new();
    public List<RecentActivityItem> RecentActivities { get; set; } = new();
    public List<TopCustomerItem> TopCustomers { get; set; } = new();
    public List<TopProductItem> TopProducts { get; set; } = new();
    public List<MonthlyTrendItem> MonthlyTrend { get; set; } = new();
}

public class SalesSummary
{
    public decimal TodayRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal YearRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int MonthOrders { get; set; }
    public int PendingInvoices { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public class PurchaseSummary
{
    public decimal TodayPurchases { get; set; }
    public decimal MonthPurchases { get; set; }
    public decimal YearPurchases { get; set; }
    public int TodayPOs { get; set; }
    public int MonthPOs { get; set; }
    public int PendingPOs { get; set; }
    public int PendingGRNs { get; set; }
}

public class InventorySummary
{
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalStockValue { get; set; }
}

public class FinancialSummary
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal AccountsReceivable { get; set; }
    public decimal AccountsPayable { get; set; }
}

public class RecentActivityItem
{
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty; // Order, Invoice, Purchase, GRN, etc.
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
}

// TopCustomerItem and TopProductItem are already defined in SalesReportResponse.cs

public class MonthlyTrendItem
{
    public string Month { get; set; } = string.Empty; // e.g., "Jan 2024"
    public decimal Sales { get; set; }
    public decimal Purchases { get; set; }
    public decimal Profit { get; set; }
}
