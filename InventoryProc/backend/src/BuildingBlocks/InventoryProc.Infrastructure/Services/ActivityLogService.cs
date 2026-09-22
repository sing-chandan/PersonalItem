using Microsoft.EntityFrameworkCore;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Identity.Application.DTOs;
using InventoryProc.Modules.Identity.Application.Services;
using InventoryProc.Modules.Identity.Domain.Entities;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly ApplicationDbContext _context;

    public ActivityLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ActivityLogResponse>>> GetActivityLogsAsync(Guid tenantId, int pageSize = 100, int page = 1)
    {
        try
        {
            var logs = await _context.ActivityLogs
                .Where(l => l.TenantId == tenantId)
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = logs.Select(l => MapToResponse(l)).ToList();
            return Result<List<ActivityLogResponse>>.Ok(response);
        }
        catch (Exception ex)
        {
            return Result<List<ActivityLogResponse>>.Fail($"Failed to retrieve activity logs: {ex.Message}");
        }
    }

    public async Task<Result<List<ActivityLogResponse>>> GetUserActivityLogsAsync(Guid userId, Guid tenantId, int pageSize = 50)
    {
        try
        {
            var logs = await _context.ActivityLogs
                .Where(l => l.TenantId == tenantId && l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Take(pageSize)
                .ToListAsync();

            var response = logs.Select(l => MapToResponse(l)).ToList();
            return Result<List<ActivityLogResponse>>.Ok(response);
        }
        catch (Exception ex)
        {
            return Result<List<ActivityLogResponse>>.Fail($"Failed to retrieve user activity logs: {ex.Message}");
        }
    }

    public async Task<Result> LogActivityAsync(
        Guid tenantId,
        Guid userId,
        string userEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? description = null,
        string? ipAddress = null)
    {
        try
        {
            var log = new ActivityLog(
                tenantId,
                userId,
                userEmail,
                action,
                entityType,
                entityId,
                description,
                ipAddress
            );

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            // Log activity failures should not block the main operation
            // Just log the error and return success
            Console.WriteLine($"Failed to log activity: {ex.Message}");
            return Result.Ok();
        }
    }

    private ActivityLogResponse MapToResponse(ActivityLog log)
    {
        return new ActivityLogResponse
        {
            Id = log.Id,
            UserId = log.UserId,
            UserEmail = log.UserEmail,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Description = log.Description,
            IpAddress = log.IpAddress,
            Timestamp = log.Timestamp
        };
    }
}
