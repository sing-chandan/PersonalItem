using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Application.Services;

public interface ICustomerService
{
    Task<Result<List<CustomerResponse>>> GetAllAsync(Guid tenantId);
    Task<Result<CustomerResponse>> GetByIdAsync(Guid id, Guid tenantId);
    Task<Result<CustomerResponse>> GetByCodeAsync(string customerCode, Guid tenantId);
    Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request, Guid tenantId, Guid userId);
    Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request, Guid tenantId, Guid userId);
    Task<Result> DeleteAsync(Guid id, Guid tenantId);
    Task<Result<List<CustomerResponse>>> SearchAsync(string searchTerm, Guid tenantId);
}
