namespace InventoryProc.Modules.Purchases.Domain.Entities;

public class PurchaseOrderItem
{
    public Guid Id { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductCode { get; private set; }
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public string? Notes { get; set; }

    // Navigation property
    public PurchaseOrder? PurchaseOrder { get; set; }

    private PurchaseOrderItem() { } // EF Core

    public PurchaseOrderItem(
        Guid productId,
        string productName,
        string productCode,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal taxRate)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        ProductCode = productCode;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        ReceivedQuantity = 0;

        CalculateTotals();
    }

    private void CalculateTotals()
    {
        TotalPrice = Quantity * UnitPrice;
        TaxAmount = TotalPrice * (TaxRate / 100);
    }

    public void UpdateQuantity(decimal newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
        CalculateTotals();
    }

    public void UpdatePrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(newUnitPrice));

        UnitPrice = newUnitPrice;
        CalculateTotals();
    }

    public void RecordReceivedQuantity(decimal receivedQty)
    {
        if (receivedQty < 0)
            throw new ArgumentException("Received quantity cannot be negative", nameof(receivedQty));

        if (ReceivedQuantity + receivedQty > Quantity)
            throw new InvalidOperationException("Total received quantity cannot exceed ordered quantity");

        ReceivedQuantity += receivedQty;
    }

    public decimal GetPendingQuantity()
    {
        return Quantity - ReceivedQuantity;
    }

    public bool IsFullyReceived()
    {
        return ReceivedQuantity >= Quantity;
    }
}
