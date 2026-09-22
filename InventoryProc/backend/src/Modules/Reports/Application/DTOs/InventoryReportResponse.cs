namespace InventoryProc.Modules.Reports.Application.DTOs;

public class InventoryReportRequest
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public bool? LowStockOnly { get; set; }
}

public class InventoryReportResponse
{
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public List<StockLevelItem> StockLevels { get; set; } = new();
    public List<LowStockItem> LowStockItems { get; set; } = new();
    public List<StockMovementItem> RecentMovements { get; set; } = new();
}

public class StockLevelItem
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal StockValue { get; set; }
}

public class LowStockItem
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int ShortageQuantity { get; set; }
}

public class StockMovementItem
{
    public DateTime Date { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty; // Sale, Purchase, Adjustment
    public int Quantity { get; set; } // Negative for outgoing, positive for incoming
    public string Reference { get; set; } = string.Empty;
}
