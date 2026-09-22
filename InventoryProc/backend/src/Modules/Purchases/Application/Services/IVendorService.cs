using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Purchases.Application.Services;

public interface IVendorService
{
    Task<Result<List<VendorResponse>>> GetAllAsync(Guid tenantId);
    Task<Result<VendorResponse>> GetByIdAsync(Guid id, Guid tenantId);
    Task<Result<VendorResponse>> GetByCodeAsync(string code, Guid tenantId);
    Task<Result<VendorResponse>> CreateAsync(CreateVendorRequest request, Guid tenantId, Guid userId);
    Task<Result<VendorResponse>> UpdateAsync(Guid id, UpdateVendorRequest request, Guid tenantId, Guid userId);
    Task<Result<bool>> DeleteAsync(Guid id, Guid tenantId);
    Task<Result<List<VendorResponse>>> SearchAsync(string searchTerm, Guid tenantId);
}
