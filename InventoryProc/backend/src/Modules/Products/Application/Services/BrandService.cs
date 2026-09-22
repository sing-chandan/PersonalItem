using Microsoft.EntityFrameworkCore;
using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.Modules.Products.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.Modules.Products.Application.Services;

public class BrandService : IBrandService
{
    private readonly DbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public BrandService(DbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<BrandResponse>> CreateBrandAsync(CreateBrandRequest request)
    {
        // Check if brand code already exists
        var existingBrand = await _context.Set<Brand>()
            .FirstOrDefaultAsync(b => b.Code == request.Code);

        if (existingBrand != null)
            return Result<BrandResponse>.Fail("Brand code already exists");

        var brand = new Brand(
            _tenantService.TenantId,
            request.Name,
            request.Code
        );

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            brand.UpdateDetails(request.Name, request.Description);
        }

        _context.Set<Brand>().Add(brand);
        await _context.SaveChangesAsync();

        var response = await MapToResponseAsync(brand);
        return Result<BrandResponse>.Ok(response, "Brand created successfully");
    }

    public async Task<Result<BrandResponse>> GetBrandByIdAsync(Guid id)
    {
        var brand = await _context.Set<Brand>()
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (brand == null)
            return Result<BrandResponse>.Fail("Brand not found");

        var response = MapToResponse(brand);
        return Result<BrandResponse>.Ok(response);
    }

    public async Task<Result<IEnumerable<BrandResponse>>> GetAllBrandsAsync()
    {
        var brands = await _context.Set<Brand>()
            .Include(b => b.Products)
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();

        var responses = brands.Select(MapToResponse);
        return Result<IEnumerable<BrandResponse>>.Ok(responses);
    }

    public async Task<Result<BrandResponse>> UpdateBrandAsync(Guid id, UpdateBrandRequest request)
    {
        var brand = await _context.Set<Brand>().FindAsync(id);

        if (brand == null)
            return Result<BrandResponse>.Fail("Brand not found");

        brand.UpdateDetails(request.Name, request.Description);

        await _context.SaveChangesAsync();

        var response = await MapToResponseAsync(brand);
        return Result<BrandResponse>.Ok(response, "Brand updated successfully");
    }

    public async Task<Result> DeleteBrandAsync(Guid id)
    {
        var brand = await _context.Set<Brand>()
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (brand == null)
            return Result.Fail("Brand not found");

        // Check if brand has products
        if (brand.Products.Any())
            return Result.Fail("Cannot delete brand with associated products");

        brand.Deactivate();
        await _context.SaveChangesAsync();

        return Result.Ok("Brand deleted successfully");
    }

    private async Task<BrandResponse> MapToResponseAsync(Brand brand)
    {
        await _context.Entry(brand).Collection(b => b.Products).LoadAsync();
        return MapToResponse(brand);
    }

    private static BrandResponse MapToResponse(Brand brand)
    {
        return new BrandResponse
        {
            Id = brand.Id,
            Name = brand.Name,
            Code = brand.Code,
            Description = brand.Description,
            IsActive = brand.IsActive,
            ProductCount = brand.Products.Count,
            CreatedAt = brand.CreatedAt
        };
    }
}
