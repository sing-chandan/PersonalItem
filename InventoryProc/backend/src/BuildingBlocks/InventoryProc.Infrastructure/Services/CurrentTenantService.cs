using InventoryProc.SharedKernel.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InventoryProc.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("tenantId")?.Value;

            return Guid.TryParse(tenantIdClaim, out var tenantId)
                ? tenantId
                : Guid.Empty;
        }
        set { } // No-op setter for backward compatibility
    }

    public bool IsSet => TenantId != Guid.Empty;
}
