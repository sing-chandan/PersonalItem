namespace InventoryProc.Modules.Sales.Application.DTOs;

public class UpdateSalesOrderRequest
{
    public DateTime OrderDate { get; set; }
    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<UpdateSalesOrderItemRequest> Items { get; set; } = new();
}

public class UpdateSalesOrderItemRequest
{
    public Guid? Id { get; set; } // Null for new items
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public string? Notes { get; set; }
}
