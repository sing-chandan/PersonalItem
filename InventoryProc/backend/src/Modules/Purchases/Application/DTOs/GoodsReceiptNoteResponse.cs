using InventoryProc.Modules.Purchases.Domain.Entities;

namespace InventoryProc.Modules.Purchases.Application.DTOs;

public class GoodsReceiptNoteResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }
    public GRNStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<GoodsReceiptNoteItemResponse> Items { get; set; } = new();
}

public class GoodsReceiptNoteItemResponse
{
    public Guid Id { get; set; }
    public Guid GoodsReceiptNoteId { get; set; }
    public Guid PurchaseOrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}

public class CreateGoodsReceiptNoteRequest
{
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }
    public List<CreateGoodsReceiptNoteItemRequest> Items { get; set; } = new();
}

public class CreateGoodsReceiptNoteItemRequest
{
    public Guid PurchaseOrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}
