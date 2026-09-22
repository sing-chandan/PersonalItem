namespace InventoryProc.SharedKernel.Common;

/// <summary>
/// Interface for auditable entities with creation and update tracking
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}
