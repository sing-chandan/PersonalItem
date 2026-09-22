using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Purchases.Domain.Entities;

public class PurchaseOrder : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public string OrderNumber { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public Guid VendorId { get; private set; }
    public string VendorName { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; set; }
    public PurchaseOrderStatus Status { get; private set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    private readonly List<PurchaseOrderItem> _items = new();
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    // Navigation property
    public Vendor? Vendor { get; set; }

    private PurchaseOrder() { } // EF Core

    public PurchaseOrder(Guid tenantId, string orderNumber, DateTime orderDate, Guid vendorId, string vendorName)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be empty", nameof(orderNumber));

        if (string.IsNullOrWhiteSpace(vendorName))
            throw new ArgumentException("Vendor name cannot be empty", nameof(vendorName));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        OrderNumber = orderNumber;
        OrderDate = orderDate;
        VendorId = vendorId;
        VendorName = vendorName;
        SubTotal = 0;
        TaxAmount = 0;
        DiscountAmount = 0;
        TotalAmount = 0;
        Status = PurchaseOrderStatus.Draft;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(PurchaseOrderItem item)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft purchase order");

        _items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Cannot remove items from a non-draft purchase order");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
        }
    }

    public void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.TotalPrice);
        TaxAmount = _items.Sum(i => i.TaxAmount);
        TotalAmount = SubTotal + TaxAmount - DiscountAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        if (discountAmount > SubTotal)
            throw new ArgumentException("Discount cannot exceed subtotal", nameof(discountAmount));

        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    public void Approve(string approvedBy)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only draft purchase orders can be approved");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot approve a purchase order without items");

        Status = PurchaseOrderStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPartiallyReceived()
    {
        if (Status != PurchaseOrderStatus.Approved && Status != PurchaseOrderStatus.PartiallyReceived)
            throw new InvalidOperationException("Cannot mark as partially received unless approved");

        Status = PurchaseOrderStatus.PartiallyReceived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFullyReceived()
    {
        if (Status != PurchaseOrderStatus.Approved && Status != PurchaseOrderStatus.PartiallyReceived)
            throw new InvalidOperationException("Cannot mark as fully received unless approved or partially received");

        Status = PurchaseOrderStatus.FullyReceived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status != PurchaseOrderStatus.FullyReceived)
            throw new InvalidOperationException("Only fully received purchase orders can be closed");

        Status = PurchaseOrderStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Closed)
            throw new InvalidOperationException("Cannot cancel a closed purchase order");

        Status = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PurchaseOrderStatus
{
    Draft,
    Approved,
    PartiallyReceived,
    FullyReceived,
    Closed,
    Cancelled
}
