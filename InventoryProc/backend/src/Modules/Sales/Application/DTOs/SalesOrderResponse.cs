using InventoryProc.Modules.Sales.Domain.Entities;

namespace InventoryProc.Modules.Sales.Application.DTOs;

public class SalesOrderResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public SalesOrderStatus Status { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<SalesOrderItemResponse> Items { get; set; } = new();
}

public class SalesOrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
}
