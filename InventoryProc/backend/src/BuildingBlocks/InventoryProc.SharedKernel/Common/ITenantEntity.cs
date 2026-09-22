namespace InventoryProc.SharedKernel.Common;

/// <summary>
/// Interface for tenant-owned entities
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
