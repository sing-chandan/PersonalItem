using InventoryProc.Modules.Products.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Products.Application.Services;

public interface IProductService
{
    Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request);
    Task<Result<ProductResponse>> GetProductByIdAsync(Guid id);
    Task<Result<IEnumerable<ProductResponse>>> GetAllProductsAsync();
    Task<Result<IEnumerable<ProductResponse>>> SearchProductsAsync(string searchTerm);
    Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<Result> DeleteProductAsync(Guid id);
    Task<Result> AdjustStockAsync(Guid id, StockAdjustmentRequest request);
    Task<Result<IEnumerable<ProductResponse>>> GetLowStockProductsAsync();
}
