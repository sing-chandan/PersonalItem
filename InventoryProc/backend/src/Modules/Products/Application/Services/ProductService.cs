using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.Modules.Products.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.Modules.Products.Application.Services;

public class ProductService : IProductService
{
    private readonly DbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(DbContext context, ICurrentTenantService tenantService, ILogger<ProductService> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        // Check if product code already exists
        var existingProduct = await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Code == request.Code);

        if (existingProduct != null)
            return Result<ProductResponse>.Fail("Product code already exists");

        // Check if category exists
        var category = await _context.Set<Category>()
            .FindAsync(request.CategoryId);

        if (category == null)
            return Result<ProductResponse>.Fail("Category not found");

        // Check brand if provided
        if (request.BrandId.HasValue)
        {
            var brand = await _context.Set<Brand>()
                .FindAsync(request.BrandId.Value);

            if (brand == null)
                return Result<ProductResponse>.Fail("Brand not found");
        }

        var product = new Product(
            _tenantService.TenantId,
            request.Name,
            request.Code,
            request.Unit,
            request.PurchasePrice,
            request.SalePrice,
            request.MRP,
            request.CategoryId
        );

        product.UpdateDetails(
            request.Name,
            request.Description,
            request.Barcode,
            request.CategoryId,
            request.BrandId,
            request.Unit,
            request.PurchasePrice,
            request.SalePrice,
            request.MRP,
            request.TaxType,
            request.TaxRate,
            request.HSNCode
        );

        product.SetStockLevels(request.MinStockLevel, request.MaxStockLevel);

        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync();

        var response = await MapToResponseAsync(product);
        return Result<ProductResponse>.Ok(response, "Product created successfully");
    }

    public async Task<Result<ProductResponse>> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return Result<ProductResponse>.Fail("Product not found");

        var response = MapToResponse(product);
        return Result<ProductResponse>.Ok(response);
    }

    public async Task<Result<IEnumerable<ProductResponse>>> GetAllProductsAsync()
    {
        var tenantId = _tenantService.TenantId;
        _logger.LogInformation("GetAllProductsAsync - TenantId: {TenantId}, IsSet: {IsSet}", tenantId, _tenantService.IsSet);

        // Test query with and without filter
        var totalProducts = await _context.Set<Product>().IgnoreQueryFilters().CountAsync();
        var filteredProducts = await _context.Set<Product>().CountAsync();
        _logger.LogInformation("Total products (no filter): {Total}, Filtered products: {Filtered}", totalProducts, filteredProducts);

        var products = await _context.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} products", products.Count);

        var responses = products.Select(MapToResponse);
        return Result<IEnumerable<ProductResponse>>.Ok(responses);
    }

    public async Task<Result<IEnumerable<ProductResponse>>> SearchProductsAsync(string searchTerm)
    {
        var products = await _context.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive &&
                   (p.Name.Contains(searchTerm) ||
                    p.Code.Contains(searchTerm) ||
                    (p.Barcode != null && p.Barcode.Contains(searchTerm))))
            .OrderBy(p => p.Name)
            .ToListAsync();

        var responses = products.Select(MapToResponse);
        return Result<IEnumerable<ProductResponse>>.Ok(responses);
    }

    public async Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _context.Set<Product>().FindAsync(id);

        if (product == null)
            return Result<ProductResponse>.Fail("Product not found");

        // Check if category exists
        var category = await _context.Set<Category>()
            .FindAsync(request.CategoryId);

        if (category == null)
            return Result<ProductResponse>.Fail("Category not found");

        // Check brand if provided
        if (request.BrandId.HasValue)
        {
            var brand = await _context.Set<Brand>()
                .FindAsync(request.BrandId.Value);

            if (brand == null)
                return Result<ProductResponse>.Fail("Brand not found");
        }

        product.UpdateDetails(
            request.Name,
            request.Description,
            request.Barcode,
            request.CategoryId,
            request.BrandId,
            request.Unit,
            request.PurchasePrice,
            request.SalePrice,
            request.MRP,
            request.TaxType,
            request.TaxRate,
            request.HSNCode
        );

        product.SetStockLevels(request.MinStockLevel, request.MaxStockLevel);

        await _context.SaveChangesAsync();

        var response = await MapToResponseAsync(product);
        return Result<ProductResponse>.Ok(response, "Product updated successfully");
    }

    public async Task<Result> DeleteProductAsync(Guid id)
    {
        var product = await _context.Set<Product>().FindAsync(id);

        if (product == null)
            return Result.Fail("Product not found");

        product.Deactivate();
        await _context.SaveChangesAsync();

        return Result.Ok("Product deleted successfully");
    }

    public async Task<Result> AdjustStockAsync(Guid id, StockAdjustmentRequest request)
    {
        var product = await _context.Set<Product>().FindAsync(id);

        if (product == null)
            return Result.Fail("Product not found");

        product.UpdateStock(request.Quantity);
        await _context.SaveChangesAsync();

        return Result.Ok("Stock adjusted successfully");
    }

    public async Task<Result<IEnumerable<ProductResponse>>> GetLowStockProductsAsync()
    {
        var products = await _context.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive &&
                   p.MinStockLevel.HasValue &&
                   p.CurrentStock <= p.MinStockLevel.Value)
            .OrderBy(p => p.CurrentStock)
            .ToListAsync();

        var responses = products.Select(MapToResponse);
        return Result<IEnumerable<ProductResponse>>.Ok(responses);
    }

    private async Task<ProductResponse> MapToResponseAsync(Product product)
    {
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        if (product.BrandId.HasValue)
            await _context.Entry(product).Reference(p => p.Brand!).LoadAsync();

        return MapToResponse(product);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Code = product.Code,
            Description = product.Description,
            Barcode = product.Barcode,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            Unit = product.Unit,
            PurchasePrice = product.PurchasePrice,
            SalePrice = product.SalePrice,
            MRP = product.MRP,
            CurrentStock = product.CurrentStock,
            MinStockLevel = product.MinStockLevel,
            MaxStockLevel = product.MaxStockLevel,
            TaxType = product.TaxType,
            TaxRate = product.TaxRate,
            HSNCode = product.HSNCode,
            IsActive = product.IsActive,
            IsLowStock = product.IsLowStock(),
            IsOutOfStock = product.IsOutOfStock(),
            CreatedAt = product.CreatedAt
        };
    }
}
