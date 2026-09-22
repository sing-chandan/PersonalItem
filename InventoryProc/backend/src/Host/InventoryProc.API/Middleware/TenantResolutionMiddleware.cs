using InventoryProc.SharedKernel.Services;
using System.Security.Claims;

namespace InventoryProc.API.Middleware;

/// <summary>
/// Middleware to resolve tenant from JWT token
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        // Verify tenant ID is available from JWT claims
        if (tenantService.IsSet)
        {
            _logger.LogDebug("Tenant resolved: {TenantId}", tenantService.TenantId);
        }
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Authenticated request without tenant ID claim");
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register middleware
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
