namespace InventoryProc.SharedKernel.Services;

/// <summary>
/// Service to get current tenant context
/// </summary>
public interface ICurrentTenantService
{
    Guid TenantId { get; set; }
    bool IsSet { get; }
}
