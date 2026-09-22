using FluentAssertions;
using InventoryProc.Modules.Products.Domain.Entities;

namespace InventoryProc.Tests.Domain;

public class ProductTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateProduct()
    {
        // Arrange & Act
        var product = new Product(
            _tenantId,
            "Test Product",
            "PROD-001",
            "PCS",
            1000m,
            1500m,
            1800m,
            _categoryId
        );

        // Assert
        product.Should().NotBeNull();
        product.TenantId.Should().Be(_tenantId);
        product.Name.Should().Be("Test Product");
        product.Code.Should().Be("PROD-001");
        product.Unit.Should().Be("PCS");
        product.PurchasePrice.Should().Be(1000m);
        product.SalePrice.Should().Be(1500m);
        product.MRP.Should().Be(1800m);
        product.CategoryId.Should().Be(_categoryId);
        product.CurrentStock.Should().Be(0); // Default
        product.IsActive.Should().BeTrue(); // Default
    }

    [Theory]
    [InlineData("", "PROD-001", "Name cannot be empty")]
    [InlineData("Product", "", "code cannot be empty")]
    [InlineData(null, "PROD-001", "Name cannot be empty")]
    [InlineData("Product", null, "code cannot be empty")]
    public void Constructor_WithInvalidData_ShouldThrowException(string name, string code, string expectedMessage)
    {
        // Arrange & Act
        Action act = () => new Product(
            _tenantId,
            name,
            code,
            "PCS",
            1000m,
            1500m,
            1800m,
            _categoryId
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*{expectedMessage}*");
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    public void Constructor_WithNegativePurchasePrice_ShouldThrowException(decimal purchasePrice)
    {
        // Arrange & Act
        Action act = () => new Product(
            _tenantId,
            "Product",
            "PROD-001",
            "PCS",
            purchasePrice,
            1500m,
            1800m,
            _categoryId
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Purchase price cannot be negative*");
    }

    [Fact]
    public void UpdateStock_ShouldIncreaseCurrentStock()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);

        // Act
        product.UpdateStock(50m);

        // Assert
        product.CurrentStock.Should().Be(50m);
    }

    [Fact]
    public void UpdateStock_MultipleUpdates_ShouldAccumulateStock()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);

        // Act
        product.UpdateStock(30m);
        product.UpdateStock(20m);
        product.UpdateStock(-10m); // Removal

        // Assert
        product.CurrentStock.Should().Be(40m); // 30 + 20 - 10
    }

    [Fact]
    public void SetStockLevels_ShouldSetMinAndMaxLevels()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);

        // Act
        product.SetStockLevels(10m, 100m);

        // Assert
        product.MinStockLevel.Should().Be(10m);
        product.MaxStockLevel.Should().Be(100m);
    }

    [Fact]
    public void IsLowStock_WhenBelowMinLevel_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);
        product.SetStockLevels(20m, 100m);
        product.UpdateStock(15m); // Below min level of 20

        // Act
        var result = product.IsLowStock();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WhenAboveMinLevel_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);
        product.SetStockLevels(10m, 100m);
        product.UpdateStock(50m); // Above min level

        // Act
        var result = product.IsLowStock();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOutOfStock_WhenStockIsZero_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);

        // Act
        var result = product.IsOutOfStock();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOutOfStock_WhenStockIsPositive_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);
        product.UpdateStock(10m);

        // Act
        var result = product.IsOutOfStock();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var product = new Product(_tenantId, "Product", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateAllFields()
    {
        // Arrange
        var product = new Product(_tenantId, "Old Name", "PROD-001", "PCS", 1000m, 1500m, 1800m, _categoryId);
        var newCategoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        // Act
        product.UpdateDetails(
            "New Name",
            "New Description",
            "BAR123",
            newCategoryId,
            brandId,
            "KG",
            1200m,
            1700m,
            2000m,
            "GST",
            18m,
            "HSN1234"
        );

        // Assert
        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New Description");
        product.Barcode.Should().Be("BAR123");
        product.CategoryId.Should().Be(newCategoryId);
        product.BrandId.Should().Be(brandId);
        product.Unit.Should().Be("KG");
        product.PurchasePrice.Should().Be(1200m);
        product.SalePrice.Should().Be(1700m);
        product.MRP.Should().Be(2000m);
        product.TaxType.Should().Be("GST");
        product.TaxRate.Should().Be(18m);
        product.HSNCode.Should().Be("HSN1234");
    }
}
