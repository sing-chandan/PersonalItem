namespace InventoryProc.Modules.Sales.Application.DTOs;

public class CreateSalesOrderRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<CreateSalesOrderItemRequest> Items { get; set; } = new();
}

public class CreateSalesOrderItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public string? Notes { get; set; }
}
