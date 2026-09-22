using InventoryProc.Modules.Identity.Application.DTOs;
using InventoryProc.Modules.Identity.Domain.Enums;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Modules.Identity.Application.Services;

public interface IUserManagementService
{
    Task<Result<List<UserResponse>>> GetAllUsersAsync(Guid tenantId);
    Task<Result<UserResponse>> GetUserByIdAsync(Guid id, Guid tenantId);
    Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request, Guid tenantId, Guid createdBy);
    Task<Result<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request, Guid tenantId, Guid updatedBy);
    Task<Result<UserResponse>> UpdateProfileAsync(Guid id, UpdateUserProfileRequest request, Guid tenantId);
    Task<Result> ChangePasswordAsync(Guid id, ChangePasswordRequest request, Guid tenantId);
    Task<Result> ActivateUserAsync(Guid id, Guid tenantId, Guid activatedBy);
    Task<Result> DeactivateUserAsync(Guid id, Guid tenantId, Guid deactivatedBy);
    Task<Result> DeleteUserAsync(Guid id, Guid tenantId, Guid deletedBy);
}
