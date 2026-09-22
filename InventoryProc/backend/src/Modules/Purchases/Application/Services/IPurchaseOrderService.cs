using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Purchases.Application.Services;

public interface IPurchaseOrderService
{
    Task<Result<List<PurchaseOrderResponse>>> GetAllAsync(Guid tenantId);
    Task<Result<PurchaseOrderResponse>> GetByIdAsync(Guid id, Guid tenantId);
    Task<Result<PurchaseOrderResponse>> GetByOrderNumberAsync(string orderNumber, Guid tenantId);
    Task<Result<List<PurchaseOrderResponse>>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<Result<PurchaseOrderResponse>> CreateAsync(CreatePurchaseOrderRequest request, Guid tenantId, Guid userId);
    Task<Result<PurchaseOrderResponse>> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, Guid tenantId, Guid userId);
    Task<Result<bool>> DeleteAsync(Guid id, Guid tenantId);
    Task<Result<PurchaseOrderResponse>> ApproveAsync(Guid id, Guid tenantId, Guid userId, string approvedBy);
    Task<Result<PurchaseOrderResponse>> CloseAsync(Guid id, Guid tenantId, Guid userId);
    Task<Result<PurchaseOrderResponse>> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<Result<PurchaseOrderResponse>> ApplyDiscountAsync(Guid id, decimal discountAmount, Guid tenantId, Guid userId);
}
