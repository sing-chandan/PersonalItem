namespace InventoryProc.Modules.Reports.Application.DTOs;

public class PurchaseReportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? ProductId { get; set; }
}

public class PurchaseReportResponse
{
    public decimal TotalPurchases { get; set; }
    public int TotalPurchaseOrders { get; set; }
    public int TotalGRNs { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalPending { get; set; }
    public List<PurchasesByPeriodItem> PurchasesByPeriod { get; set; } = new();
    public List<TopVendorItem> TopVendors { get; set; } = new();
    public List<TopPurchaseProductItem> TopProducts { get; set; } = new();
}

public class PurchasesByPeriodItem
{
    public DateTime Date { get; set; }
    public decimal PurchaseAmount { get; set; }
    public int OrderCount { get; set; }
}

public class TopVendorItem
{
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public int OrderCount { get; set; }
    public decimal OutstandingBalance { get; set; }
}

public class TopPurchaseProductItem
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantityPurchased { get; set; }
    public decimal PurchaseAmount { get; set; }
}
