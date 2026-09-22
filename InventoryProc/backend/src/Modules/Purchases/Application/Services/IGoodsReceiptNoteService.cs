using InventoryProc.Modules.Purchases.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Purchases.Application.Services;

public interface IGoodsReceiptNoteService
{
    Task<Result<List<GoodsReceiptNoteResponse>>> GetAllAsync(Guid tenantId);
    Task<Result<GoodsReceiptNoteResponse>> GetByIdAsync(Guid id, Guid tenantId);
    Task<Result<GoodsReceiptNoteResponse>> GetByGRNNumberAsync(string grnNumber, Guid tenantId);
    Task<Result<List<GoodsReceiptNoteResponse>>> GetByPurchaseOrderAsync(Guid purchaseOrderId, Guid tenantId);
    Task<Result<GoodsReceiptNoteResponse>> CreateAsync(CreateGoodsReceiptNoteRequest request, Guid tenantId, Guid userId);
    Task<Result<GoodsReceiptNoteResponse>> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<Result<GoodsReceiptNoteResponse>> CancelAsync(Guid id, Guid tenantId, Guid userId);
    Task<Result<bool>> DeleteAsync(Guid id, Guid tenantId);
}
