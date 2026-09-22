using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Domain.Entities;

/// <summary>
/// Invoice payment record
/// </summary>
public class InvoicePayment : Entity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Cheque, Bank Transfer, Card, UPI, etc.
    public string? Reference { get; set; } // Transaction ID, Cheque number, etc.
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Invoice? Invoice { get; set; }

    private InvoicePayment() { } // EF Core

    public InvoicePayment(
        Guid invoiceId,
        decimal amount,
        string paymentMethod,
        DateTime paymentDate)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));

        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method cannot be empty", nameof(paymentMethod));

        InvoiceId = invoiceId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        PaymentDate = paymentDate;
        CreatedAt = DateTime.UtcNow;
    }
}
