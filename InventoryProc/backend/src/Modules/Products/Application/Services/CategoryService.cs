using Microsoft.EntityFrameworkCore;
using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.Modules.Products.Domain.Entities;
using InventoryProc.SharedKernel.Common;
using InventoryProc.SharedKernel.Services;

namespace InventoryProc.Modules.Products.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly DbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CategoryService(DbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var existingCategory = await _context.Set<Category>()
            .FirstOrDefaultAsync(c => c.Code == request.Code);

        if (existingCategory != null)
            return Result<CategoryResponse>.Fail("Category code already exists");

        var category = new Category(
            _tenantService.TenantId,
            request.Name,
            request.Code,
            request.ParentCategoryId
        );

        category.UpdateDetails(request.Name, request.Description, request.ParentCategoryId);

        _context.Set<Category>().Add(category);
        await _context.SaveChangesAsync();

        var response = MapToResponse(category);
        return Result<CategoryResponse>.Ok(response, "Category created successfully");
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Set<Category>()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return Result<CategoryResponse>.Fail("Category not found");

        var response = MapToResponse(category);
        return Result<CategoryResponse>.Ok(response);
    }

    public async Task<Result<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync()
    {
        var categories = await _context.Set<Category>()
            .Include(c => c.ParentCategory)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var responses = categories.Select(MapToResponse);
        return Result<IEnumerable<CategoryResponse>>.Ok(responses);
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _context.Set<Category>().FindAsync(id);

        if (category == null)
            return Result<CategoryResponse>.Fail("Category not found");

        category.UpdateDetails(request.Name, request.Description, request.ParentCategoryId);

        await _context.SaveChangesAsync();

        var response = MapToResponse(category);
        return Result<CategoryResponse>.Ok(response, "Category updated successfully");
    }

    public async Task<Result> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Set<Category>().FindAsync(id);

        if (category == null)
            return Result.Fail("Category not found");

        var hasProducts = await _context.Set<Product>()
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
            return Result.Fail("Cannot delete category with associated products");

        category.Deactivate();
        await _context.SaveChangesAsync();

        return Result.Ok("Category deleted successfully");
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            IsActive = category.IsActive,
            ProductCount = category.Products.Count,
            CreatedAt = category.CreatedAt
        };
    }
}
