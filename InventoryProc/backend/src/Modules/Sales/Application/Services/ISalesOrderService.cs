using InventoryProc.Modules.Sales.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Sales.Application.Services;

public interface ISalesOrderService
{
    Task<Result<List<SalesOrderResponse>>> GetAllAsync(Guid tenantId);
    Task<Result<SalesOrderResponse>> GetByIdAsync(Guid id, Guid tenantId);
    Task<Result<SalesOrderResponse>> GetByOrderNumberAsync(string orderNumber, Guid tenantId);
    Task<Result<SalesOrderResponse>> CreateAsync(CreateSalesOrderRequest request, Guid tenantId, Guid userId);
    Task<Result<SalesOrderResponse>> UpdateAsync(Guid id, UpdateSalesOrderRequest request, Guid tenantId, Guid userId);
    Task<Result> DeleteAsync(Guid id, Guid tenantId);
    Task<Result<SalesOrderResponse>> ConfirmAsync(Guid id, Guid tenantId, Guid userId);
    Task<Result<SalesOrderResponse>> ShipAsync(Guid id, Guid tenantId, Guid userId, DateTime? shippedDate = null);
    Task<Result<SalesOrderResponse>> DeliverAsync(Guid id, Guid tenantId, Guid userId, DateTime? deliveredDate = null);
    Task<Result<SalesOrderResponse>> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<Result<SalesOrderResponse>> ApplyDiscountAsync(Guid id, decimal discountAmount, Guid tenantId, Guid userId);
    Task<Result<List<SalesOrderResponse>>> GetByCustomerAsync(Guid customerId, Guid tenantId);
}
