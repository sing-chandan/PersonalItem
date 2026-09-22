namespace InventoryProc.Modules.Purchases.Domain.Entities;

public class GoodsReceiptNoteItem
{
    public Guid Id { get; private set; }
    public Guid GoodsReceiptNoteId { get; private set; }
    public Guid PurchaseOrderItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductCode { get; private set; }
    public decimal OrderedQuantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal AcceptedQuantity { get; private set; }
    public decimal RejectedQuantity { get; private set; }
    public string Unit { get; private set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation property
    public GoodsReceiptNote? GoodsReceiptNote { get; set; }

    private GoodsReceiptNoteItem() { } // EF Core

    public GoodsReceiptNoteItem(
        Guid purchaseOrderItemId,
        Guid productId,
        string productName,
        string productCode,
        decimal orderedQuantity,
        decimal receivedQuantity,
        string unit)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (orderedQuantity <= 0)
            throw new ArgumentException("Ordered quantity must be greater than zero", nameof(orderedQuantity));

        if (receivedQuantity < 0)
            throw new ArgumentException("Received quantity cannot be negative", nameof(receivedQuantity));

        if (receivedQuantity > orderedQuantity)
            throw new ArgumentException("Received quantity cannot exceed ordered quantity", nameof(receivedQuantity));

        Id = Guid.NewGuid();
        PurchaseOrderItemId = purchaseOrderItemId;
        ProductId = productId;
        ProductName = productName;
        ProductCode = productCode;
        OrderedQuantity = orderedQuantity;
        ReceivedQuantity = receivedQuantity;
        AcceptedQuantity = receivedQuantity; // Initially all accepted
        RejectedQuantity = 0;
        Unit = unit;
    }

    public void RecordQualityCheck(decimal acceptedQty, decimal rejectedQty, string? rejectionReason = null)
    {
        if (acceptedQty < 0)
            throw new ArgumentException("Accepted quantity cannot be negative", nameof(acceptedQty));

        if (rejectedQty < 0)
            throw new ArgumentException("Rejected quantity cannot be negative", nameof(rejectedQty));

        if (acceptedQty + rejectedQty != ReceivedQuantity)
            throw new ArgumentException("Sum of accepted and rejected quantities must equal received quantity");

        AcceptedQuantity = acceptedQty;
        RejectedQuantity = rejectedQty;

        if (rejectedQty > 0 && string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required when items are rejected", nameof(rejectionReason));

        RejectionReason = rejectionReason;
    }

    public decimal GetPendingQuantity()
    {
        return OrderedQuantity - ReceivedQuantity;
    }
}
