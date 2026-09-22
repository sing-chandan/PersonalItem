using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Purchases.Domain.Entities;

public class GoodsReceiptNote : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public string GRNNumber { get; private set; }
    public DateTime ReceiptDate { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public string PurchaseOrderNumber { get; private set; }
    public Guid VendorId { get; private set; }
    public string VendorName { get; private set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }
    public GRNStatus Status { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    private readonly List<GoodsReceiptNoteItem> _items = new();
    public IReadOnlyCollection<GoodsReceiptNoteItem> Items => _items.AsReadOnly();

    // Navigation properties
    public PurchaseOrder? PurchaseOrder { get; set; }
    public Vendor? Vendor { get; set; }

    private GoodsReceiptNote() { } // EF Core

    public GoodsReceiptNote(
        Guid tenantId,
        string grnNumber,
        DateTime receiptDate,
        Guid purchaseOrderId,
        string purchaseOrderNumber,
        Guid vendorId,
        string vendorName)
    {
        if (string.IsNullOrWhiteSpace(grnNumber))
            throw new ArgumentException("GRN number cannot be empty", nameof(grnNumber));

        if (string.IsNullOrWhiteSpace(purchaseOrderNumber))
            throw new ArgumentException("Purchase order number cannot be empty", nameof(purchaseOrderNumber));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        GRNNumber = grnNumber;
        ReceiptDate = receiptDate;
        PurchaseOrderId = purchaseOrderId;
        PurchaseOrderNumber = purchaseOrderNumber;
        VendorId = vendorId;
        VendorName = vendorName;
        Status = GRNStatus.Draft;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(GoodsReceiptNoteItem item)
    {
        if (Status != GRNStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft GRN");

        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != GRNStatus.Draft)
            throw new InvalidOperationException("Cannot remove items from a non-draft GRN");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Complete()
    {
        if (Status != GRNStatus.Draft)
            throw new InvalidOperationException("Only draft GRNs can be completed");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot complete a GRN without items");

        Status = GRNStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == GRNStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed GRN");

        Status = GRNStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum GRNStatus
{
    Draft,
    Completed,
    Cancelled
}
