using FluentAssertions;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.Modules.Products.Application.Services;
using InventoryProc.Modules.Products.Domain.Entities;
using InventoryProc.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryProc.Tests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _userServiceMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _sut;
    private readonly Guid _tenantId;
    private readonly Guid _userId;

    public ProductServiceTests()
    {
        _tenantId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        _tenantServiceMock = new Mock<ICurrentTenantService>();
        _tenantServiceMock.Setup(x => x.TenantId).Returns(_tenantId);
        _tenantServiceMock.Setup(x => x.IsSet).Returns(true);

        _userServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock.Setup(x => x.UserId).Returns(_userId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantServiceMock.Object, _userServiceMock.Object);

        _loggerMock = new Mock<ILogger<ProductService>>();
        _sut = new ProductService(_context, _tenantServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenProductsExist_ShouldReturnProducts()
    {
        // Arrange
        var category = new Category(_tenantId, "Electronics", "ELEC", null)
        {
            CreatedBy = _userId
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product(_tenantId, "Test Product", "TEST001", "PCS", 100, 150, 200, category.Id)
        {
            Description = "Test Description",
            CurrentStock = 50,
            CreatedBy = _userId
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        var productResponse = result.Data.First();
        productResponse.Name.Should().Be("Test Product");
        productResponse.Code.Should().Be("TEST001");
        productResponse.CategoryName.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetAllProductsAsync_WithDifferentTenant_ShouldReturnEmpty()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var category = new Category(otherTenantId, "Electronics", "ELEC", null)
        {
            CreatedBy = _userId
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product(otherTenantId, "Other Tenant Product", "OTHER001", "PCS", 100, 150, 200, category.Id)
        {
            CreatedBy = _userId
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenExists_ShouldReturnProduct()
    {
        // Arrange
        var category = new Category(_tenantId, "Electronics", "ELEC", null)
        {
            CreatedBy = _userId
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product(_tenantId, "Test Product", "TEST001", "PCS", 100, 150, 200, category.Id)
        {
            CreatedBy = _userId
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetProductByIdAsync(product.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(product.Id);
        result.Data.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task CreateProductAsync_WhenValid_ShouldCreateProduct()
    {
        // Arrange
        var category = new Category(_tenantId, "Electronics", "ELEC", null)
        {
            CreatedBy = _userId
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var request = new CreateProductRequest
        {
            Name = "New Product",
            Code = "NEW001",
            CategoryId = category.Id,
            Unit = "PCS",
            PurchasePrice = 100,
            SalePrice = 150,
            MRP = 200
        };

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Product");
        result.Data.Code.Should().Be("NEW001");

        // Verify in database
        var savedProduct = await _context.Products.FirstOrDefaultAsync(p => p.Code == "NEW001");
        savedProduct.Should().NotBeNull();
        savedProduct!.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task CreateProductAsync_WhenCodeExists_ShouldReturnFailure()
    {
        // Arrange
        var category = new Category(_tenantId, "Electronics", "ELEC", null)
        {
            CreatedBy = _userId
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var existingProduct = new Product(_tenantId, "Existing", "DUPLICATE", "PCS", 100, 150, 200, category.Id)
        {
            CreatedBy = _userId
        };
        _context.Products.Add(existingProduct);
        await _context.SaveChangesAsync();

        var request = new CreateProductRequest
        {
            Name = "New Product",
            Code = "DUPLICATE",
            CategoryId = category.Id,
            Unit = "PCS",
            PurchasePrice = 100,
            SalePrice = 150,
            MRP = 200
        };

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Product code already exists");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
