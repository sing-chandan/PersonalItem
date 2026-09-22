using InventoryProc.Modules.Sales.Domain.Entities;

namespace InventoryProc.Modules.Sales.Application.DTOs;

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? ShippingAddress { get; set; }
    public string? CustomerGSTNumber { get; set; }
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCharges { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InvoiceItemResponse> Items { get; set; } = new();
    public List<InvoicePaymentResponse> Payments { get; set; } = new();
}

public class InvoiceItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string? HSNCode { get; set; }
}

public class InvoicePaymentResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInvoiceRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? ShippingAddress { get; set; }
    public string? CustomerGSTNumber { get; set; }
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCharges { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public List<CreateInvoiceItemRequest> Items { get; set; } = new();
}

public class CreateInvoiceItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public string? HSNCode { get; set; }
}

public class RecordPaymentRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime? PaymentDate { get; set; }
}
