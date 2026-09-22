using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Identity.Domain.Entities;

public class ActivityLog : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string? Description { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime Timestamp { get; private set; }
    
    private ActivityLog() { } // EF Core

    public ActivityLog(
        Guid tenantId,
        Guid userId,
        string userEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? description = null,
        string? ipAddress = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        UserEmail = userEmail ?? string.Empty;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Description = description;
        IpAddress = ipAddress;
        Timestamp = DateTime.UtcNow;
    }
}
