using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Domain.Entities;

/// <summary>
/// Sales order entity
/// </summary>
public class SalesOrder : Entity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public SalesOrderStatus Status { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();

    private SalesOrder() { } // EF Core

    public SalesOrder(
        Guid tenantId,
        string orderNumber,
        Guid customerId,
        string customerName,
        DateTime orderDate)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be empty", nameof(orderNumber));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty", nameof(customerName));

        TenantId = tenantId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        CustomerName = customerName;
        OrderDate = orderDate;
        Status = SalesOrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(SalesOrderItem item)
    {
        Items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
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
        TotalAmount = SubTotal + TaxAmount - DiscountAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be confirmed");

        if (!Items.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = SalesOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship(DateTime? shippedDate = null)
    {
        if (Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be shipped");

        Status = SalesOrderStatus.Shipped;
        ShippedDate = shippedDate ?? DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deliver(DateTime? deliveredDate = null)
    {
        if (Status != SalesOrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered");

        Status = SalesOrderStatus.Delivered;
        DeliveredDate = deliveredDate ?? DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == SalesOrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered orders");

        Status = SalesOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
