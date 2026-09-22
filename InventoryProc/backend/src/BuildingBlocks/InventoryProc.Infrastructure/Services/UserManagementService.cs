using Microsoft.EntityFrameworkCore;
using InventoryProc.Infrastructure.Persistence;
using InventoryProc.Modules.Identity.Application.DTOs;
using InventoryProc.Modules.Identity.Application.Services;
using InventoryProc.Modules.Identity.Domain.Entities;
using InventoryProc.Modules.Identity.Domain.Enums;
using InventoryProc.SharedKernel.Common;

namespace InventoryProc.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public UserManagementService(ApplicationDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<Result<List<UserResponse>>> GetAllUsersAsync(Guid tenantId)
    {
        try
        {
            var users = await _context.Users
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            var response = users.Select(u => MapToResponse(u)).ToList();
            return Result<List<UserResponse>>.Ok(response);
        }
        catch (Exception ex)
        {
            return Result<List<UserResponse>>.Fail($"Failed to retrieve users: {ex.Message}");
        }
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(Guid id, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null)
                return Result<UserResponse>.Fail("User not found");

            return Result<UserResponse>.Ok(MapToResponse(user));
        }
        catch (Exception ex)
        {
            return Result<UserResponse>.Fail($"Failed to retrieve user: {ex.Message}");
        }
    }

    public async Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request, Guid tenantId, Guid createdBy)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.TenantId == tenantId);

            if (existingUser != null)
                return Result<UserResponse>.Fail("A user with this email already exists");

            // Hash password (simple implementation - in production use proper password hashing)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User(tenantId, request.Email, passwordHash, request.FirstName, request.LastName, request.Role)
            {
                Mobile = request.Mobile,
                CreatedBy = createdBy
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Log activity
            await _activityLogService.LogActivityAsync(
                tenantId, createdBy, request.Email,
                "Create", "User", user.Id.ToString(),
                $"Created user: {user.GetFullName()}"
            );

            return Result<UserResponse>.Ok(MapToResponse(user));
        }
        catch (Exception ex)
        {
            return Result<UserResponse>.Fail($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<Result<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request, Guid tenantId, Guid updatedBy)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null)
                return Result<UserResponse>.Fail("User not found");

            user.UpdateProfile(request.FirstName, request.LastName, request.Mobile);
            user.UpdateRole(request.Role);

            if (request.IsActive && !user.IsActive)
                user.Activate();
            else if (!request.IsActive && user.IsActive)
                user.Deactivate();

            user.UpdatedBy = updatedBy;
            await _context.SaveChangesAsync();

            // Log activity
            await _activityLogService.LogActivityAsync(
                tenantId, updatedBy, user.Email,
                "Update", "User", user.Id.ToString(),
                $"Updated user: {user.GetFullName()}"
            );

            return Result<UserResponse>.Ok(MapToResponse(user));
        }
        catch (Exception ex)
        {
            return Result<UserResponse>.Fail($"Failed to update user: {ex.Message}");
        }
    }

    public async Task<Result<UserResponse>> UpdateProfileAsync(Guid id, UpdateUserProfileRequest request, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null)
                return Result<UserResponse>.Fail("User not found");

            user.UpdateProfile(request.FirstName, request.LastName, request.Mobile);
            await _context.SaveChangesAsync();

            return Result<UserResponse>.Ok(MapToResponse(user));
        }
        catch (Exception ex)
        {
            return Result<UserResponse>.Fail($"Failed to update profile: {ex.Message}");
        }
    }

    public async Task<Result> ChangePasswordAsync(Guid id, ChangePasswordRequest request, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null)
                return Result.Fail("User not found");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return Result.Fail("Current password is incorrect");

            // Hash new password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatePassword(newPasswordHash);

            await _context.SaveChangesAsync();

            // Log activity
            await _activityLogService.LogActivityAsync(
                tenantId, id, user.Email,
                "ChangePassword", "User", user.Id.ToString(),
                "Password changed"
            );

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to change password: {ex.Message}");
        }
    }

    public async Task<Result> ActivateUserAsync(Guid id, Guid tenantId, Guid activatedBy)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null)
                return Result.Fail("User not found");

            user.Activate();
            await _context.SaveChangesAsync();

            // Log activity
            await _activityLogService.LogActivityAsync(
                tenantId, activatedBy, user.Email,
                "ActivateUser", "User", user.Id.ToString(),
                $"User {user.GetFullName()} activated"
            );

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to activate user: {ex.Message}");
        }
    }

    public async Task<Result> DeactivateUserAsync(Guid id, Guid tenantId, Guid deactivatedBy)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null)
                return Result.Fail("User not found");

            user.Deactivate();
            await _context.SaveChangesAsync();

            // Log activity
            await _activityLogService.LogActivityAsync(
                tenantId, deactivatedBy, user.Email,
                "DeactivateUser", "User", user.Id.ToString(),
                $"User {user.GetFullName()} deactivated"
            );

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to deactivate user: {ex.Message}");
        }
    }

    public async Task<Result> DeleteUserAsync(Guid id, Guid tenantId, Guid deletedBy)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

            if (user == null)
                return Result.Fail("User not found");

            var userEmail = user.Email;
            var userName = user.GetFullName();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Log activity
            await _activityLogService.LogActivityAsync(
                tenantId, deletedBy, userEmail,
                "DeleteUser", "User", id.ToString(),
                $"User {userName} deleted"
            );

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete user: {ex.Message}");
        }
    }

    private UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.GetFullName(),
            Mobile = user.Mobile,
            Role = user.Role,
            RoleName = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
