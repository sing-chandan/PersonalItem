using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Domain.Entities;

/// <summary>
/// Invoice entity for sales invoicing
/// </summary>
public class Invoice : Entity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }

    // Customer Information
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? ShippingAddress { get; set; }
    public string? CustomerGSTNumber { get; set; }

    // Order Reference
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }

    // Financial Details
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCharges { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    // Payment Information
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }

    // Additional Information
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Audit
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation Properties
    public Customer? Customer { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();

    private Invoice() { } // EF Core

    public Invoice(
        Guid tenantId,
        string invoiceNumber,
        DateTime invoiceDate,
        DateTime dueDate,
        Guid customerId,
        string customerName)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number cannot be empty", nameof(invoiceNumber));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty", nameof(customerName));

        if (dueDate < invoiceDate)
            throw new ArgumentException("Due date cannot be before invoice date", nameof(dueDate));

        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        CustomerId = customerId;
        CustomerName = customerName;
        PaymentStatus = InvoicePaymentStatus.Unpaid;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(InvoiceItem item)
    {
        if (PaymentStatus == InvoicePaymentStatus.Paid)
            throw new InvalidOperationException("Cannot modify a paid invoice");

        Items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        if (PaymentStatus == InvoicePaymentStatus.Paid)
            throw new InvalidOperationException("Cannot modify a paid invoice");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotals();
        }
    }

    public void RecalculateTotals()
    {
        SubTotal = Items.Sum(i => i.TotalPrice);
        TaxAmount = Items.Sum(i => i.TaxAmount);
        TotalAmount = SubTotal + TaxAmount + ShippingCharges - DiscountAmount;
        BalanceAmount = TotalAmount - PaidAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(amount));

        if (amount > SubTotal)
            throw new ArgumentException("Discount amount cannot exceed subtotal", nameof(amount));

        DiscountAmount = amount;
        RecalculateTotals();
    }

    public void AddShippingCharges(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Shipping charges cannot be negative", nameof(amount));

        ShippingCharges = amount;
        RecalculateTotals();
    }

    public void RecordPayment(decimal amount, string paymentMethod, string? reference = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));

        if (amount > BalanceAmount)
            throw new ArgumentException("Payment amount cannot exceed balance amount", nameof(amount));

        var payment = new InvoicePayment(Id, amount, paymentMethod, DateTime.UtcNow)
        {
            Reference = reference
        };

        Payments.Add(payment);
        PaidAmount += amount;
        BalanceAmount = TotalAmount - PaidAmount;

        UpdatePaymentStatus();
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdatePaymentStatus()
    {
        if (PaidAmount >= TotalAmount)
        {
            PaymentStatus = InvoicePaymentStatus.Paid;
            PaymentDate = DateTime.UtcNow;
        }
        else if (PaidAmount > 0)
        {
            PaymentStatus = InvoicePaymentStatus.PartiallyPaid;
        }
        else
        {
            PaymentStatus = InvoicePaymentStatus.Unpaid;
        }

        // Check if overdue
        if (PaymentStatus != InvoicePaymentStatus.Paid && DateTime.UtcNow > DueDate)
        {
            PaymentStatus = InvoicePaymentStatus.Overdue;
        }
    }

    public void MarkAsVoid()
    {
        if (PaymentStatus == InvoicePaymentStatus.Paid)
            throw new InvalidOperationException("Cannot void a paid invoice");

        PaymentStatus = InvoicePaymentStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum InvoicePaymentStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}
