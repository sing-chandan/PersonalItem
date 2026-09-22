using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Domain.Entities;

/// <summary>
/// Invoice line item
/// </summary>
public class InvoiceItem : Entity
{
    public Guid InvoiceId { get; set; }
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

    // Navigation properties
    public Invoice? Invoice { get; set; }

    private InvoiceItem() { } // EF Core

    public InvoiceItem(
        Guid productId,
        string productName,
        string productCode,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal taxRate = 0)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        ProductId = productId;
        ProductName = productName;
        ProductCode = productCode;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        CalculateTotals();
    }

    public void UpdateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        Quantity = quantity;
        CalculateTotals();
    }

    public void UpdatePrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        UnitPrice = unitPrice;
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        TotalPrice = Quantity * UnitPrice;
        TaxAmount = TotalPrice * (TaxRate / 100);
    }
}
