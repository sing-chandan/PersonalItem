using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryProc.Modules.Identity.Application.Services;
using InventoryProc.Modules.Identity.Application.DTOs;
using InventoryProc.SharedKernel.Common;
using System.Security.Claims;

namespace InventoryProc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserManagementService userManagementService,
        IActivityLogService activityLogService,
        ILogger<UsersController> logger)
    {
        _userManagementService = userManagementService;
        _activityLogService = activityLogService;
        _logger = logger;
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Get all users in the tenant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var result = await _userManagementService.GetAllUsersAsync(tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, Result.Fail("An error occurred while retrieving users"));
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var result = await _userManagementService.GetUserByIdAsync(id, tenantId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, Result.Fail("An error occurred while retrieving the user"));
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var createdBy = GetUserId();
            if (createdBy == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid user"));

            var result = await _userManagementService.CreateUserAsync(request, tenantId, createdBy);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, Result.Fail("An error occurred while creating the user"));
        }
    }

    /// <summary>
    /// Update user details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var updatedBy = GetUserId();
            if (updatedBy == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid user"));

            var result = await _userManagementService.UpdateUserAsync(id, request, tenantId, updatedBy);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, Result.Fail("An error occurred while updating the user"));
        }
    }

    /// <summary>
    /// Update user profile (self-service)
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid user"));

            var result = await _userManagementService.UpdateProfileAsync(userId, request, tenantId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, Result.Fail("An error occurred while updating the profile"));
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var result = await _userManagementService.ChangePasswordAsync(id, request, tenantId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", id);
            return StatusCode(500, Result.Fail("An error occurred while changing the password"));
        }
    }

    /// <summary>
    /// Activate user
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var activatedBy = GetUserId();
            if (activatedBy == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid user"));

            var result = await _userManagementService.ActivateUserAsync(id, tenantId, activatedBy);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", id);
            return StatusCode(500, Result.Fail("An error occurred while activating the user"));
        }
    }

    /// <summary>
    /// Deactivate user
    /// </summary>
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var deactivatedBy = GetUserId();
            if (deactivatedBy == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid user"));

            var result = await _userManagementService.DeactivateUserAsync(id, tenantId, deactivatedBy);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(500, Result.Fail("An error occurred while deactivating the user"));
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var deletedBy = GetUserId();
            if (deletedBy == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid user"));

            var result = await _userManagementService.DeleteUserAsync(id, tenantId, deletedBy);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, Result.Fail("An error occurred while deleting the user"));
        }
    }

    /// <summary>
    /// Get activity logs for the tenant
    /// </summary>
    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetActivityLogs([FromQuery] int pageSize = 100, [FromQuery] int page = 1)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var result = await _activityLogService.GetActivityLogsAsync(tenantId, pageSize, page);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity logs");
            return StatusCode(500, Result.Fail("An error occurred while retrieving activity logs"));
        }
    }

    /// <summary>
    /// Get activity logs for a specific user
    /// </summary>
    [HttpGet("{userId}/activity-logs")]
    public async Task<IActionResult> GetUserActivityLogs(Guid userId, [FromQuery] int pageSize = 50)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return Unauthorized(Result.Fail("Invalid tenant"));

            var result = await _activityLogService.GetUserActivityLogsAsync(userId, tenantId, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity logs for {UserId}", userId);
            return StatusCode(500, Result.Fail("An error occurred while retrieving user activity logs"));
        }
    }
}
