namespace InventoryProc.Modules.Reports.Application.DTOs;

public class SalesReportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? ProductId { get; set; }
}

public class SalesReportResponse
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public List<SalesByPeriodItem> SalesByPeriod { get; set; } = new();
    public List<TopCustomerItem> TopCustomers { get; set; } = new();
    public List<TopProductItem> TopProducts { get; set; } = new();
}

public class SalesByPeriodItem
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopCustomerItem
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductItem
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
