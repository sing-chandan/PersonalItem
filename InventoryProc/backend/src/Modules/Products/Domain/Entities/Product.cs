using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Products.Domain.Entities;

/// <summary>
/// Product entity representing an inventory item
/// </summary>
public class Product : Entity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string Unit { get; set; } = "PCS"; // PCS, KG, LITER, etc.
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal MRP { get; set; }
    public decimal? MinStockLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    public decimal CurrentStock { get; set; }
    public string? TaxType { get; set; } // GST, VAT, None
    public decimal? TaxRate { get; set; }
    public string? HSNCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public Brand? Brand { get; set; }

    private Product() { } // EF Core

    public Product(
        Guid tenantId,
        string name,
        string code,
        string unit,
        decimal purchasePrice,
        decimal salePrice,
        decimal mrp,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Product code cannot be empty", nameof(code));

        if (purchasePrice < 0)
            throw new ArgumentException("Purchase price cannot be negative", nameof(purchasePrice));

        if (salePrice < 0)
            throw new ArgumentException("Sale price cannot be negative", nameof(salePrice));

        if (mrp < 0)
            throw new ArgumentException("MRP cannot be negative", nameof(mrp));

        TenantId = tenantId;
        Name = name;
        Code = code;
        Unit = unit;
        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
        MRP = mrp;
        CategoryId = categoryId;
        CurrentStock = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string? barcode,
        Guid categoryId,
        Guid? brandId,
        string unit,
        decimal purchasePrice,
        decimal salePrice,
        decimal mrp,
        string? taxType,
        decimal? taxRate,
        string? hsnCode)
    {
        Name = name;
        Description = description;
        Barcode = barcode;
        CategoryId = categoryId;
        BrandId = brandId;
        Unit = unit;
        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
        MRP = mrp;
        TaxType = taxType;
        TaxRate = taxRate;
        HSNCode = hsnCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(decimal quantity)
    {
        CurrentStock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStockLevels(decimal? minLevel, decimal? maxLevel)
    {
        MinStockLevel = minLevel;
        MaxStockLevel = maxLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLowStock()
    {
        return MinStockLevel.HasValue && CurrentStock <= MinStockLevel.Value;
    }

    public bool IsOutOfStock()
    {
        return CurrentStock <= 0;
    }
}
