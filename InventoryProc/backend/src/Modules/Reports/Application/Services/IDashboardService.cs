using InventoryProc.Modules.Reports.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Reports.Application.Services;

public interface IDashboardService
{
    Task<Result<DashboardSummaryResponse>> GetDashboardSummaryAsync(Guid tenantId);
}
