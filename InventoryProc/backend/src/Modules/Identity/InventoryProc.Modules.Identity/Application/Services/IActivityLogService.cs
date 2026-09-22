using InventoryProc.Modules.Identity.Application.DTOs;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Identity.Application.Services;

public interface IActivityLogService
{
    Task<Result<List<ActivityLogResponse>>> GetActivityLogsAsync(Guid tenantId, int pageSize = 100, int page = 1);
    Task<Result<List<ActivityLogResponse>>> GetUserActivityLogsAsync(Guid userId, Guid tenantId, int pageSize = 50);
    Task<Result> LogActivityAsync(
        Guid tenantId,
        Guid userId,
        string userEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? description = null,
        string? ipAddress = null);
}
