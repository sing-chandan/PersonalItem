using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Reports.Application.Services;

public interface IInventoryReportService
{
    Task<Result<InventoryReportResponse>> GetInventoryReportAsync(InventoryReportRequest request, Guid tenantId);
}
