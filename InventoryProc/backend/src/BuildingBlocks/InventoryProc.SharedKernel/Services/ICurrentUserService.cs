namespace InventoryProc.SharedKernel.Services;

/// <summary>
/// Service to get current user context
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
