using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Products.Application.Services;

public interface ICategoryService
{
    Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request);
    Task<Result<CategoryResponse>> GetCategoryByIdAsync(Guid id);
    Task<Result<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync();
    Task<Result<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<Result> DeleteCategoryAsync(Guid id);
}
