using InventoryProc.Modules.Purchases.Domain.Entities;

namespace InventoryProc.Modules.Purchases.Application.DTOs;

public class PurchaseOrderResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PurchaseOrderItemResponse> Items { get; set; } = new();
}

public class PurchaseOrderItemResponse
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemRequest
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

public class UpdatePurchaseOrderRequest
{
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<UpdatePurchaseOrderItemRequest> Items { get; set; } = new();
}

public class UpdatePurchaseOrderItemRequest
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public string? Notes { get; set; }
}
