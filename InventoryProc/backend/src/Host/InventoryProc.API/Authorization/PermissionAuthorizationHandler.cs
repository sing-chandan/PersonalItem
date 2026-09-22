using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using InventoryProc.SharedKernel.Authorization;

namespace InventoryProc.API.Authorization;

/// <summary>
/// Handles permission-based authorization checks
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user's role from claims
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            return Task.CompletedTask;
        }

        var userRole = roleClaim.Value;

        // Get permissions for the user's role
        var rolePermissions = RolePermissions.GetRolePermissions();

        if (!rolePermissions.TryGetValue(userRole, out var permissions))
        {
            return Task.CompletedTask;
        }

        // Check if user has the required permission
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
