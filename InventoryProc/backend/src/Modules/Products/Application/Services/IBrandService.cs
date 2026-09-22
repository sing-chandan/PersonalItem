using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Products.Application.Services;

public interface IBrandService
{
    Task<Result<BrandResponse>> CreateBrandAsync(CreateBrandRequest request);
    Task<Result<BrandResponse>> GetBrandByIdAsync(Guid id);
    Task<Result<IEnumerable<BrandResponse>>> GetAllBrandsAsync();
    Task<Result<BrandResponse>> UpdateBrandAsync(Guid id, UpdateBrandRequest request);
    Task<Result> DeleteBrandAsync(Guid id);
}
