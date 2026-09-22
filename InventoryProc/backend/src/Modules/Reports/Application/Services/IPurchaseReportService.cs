using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Reports.Application.Services;

public interface IPurchaseReportService
{
    Task<Result<PurchaseReportResponse>> GetPurchaseReportAsync(PurchaseReportRequest request, Guid tenantId);
}
