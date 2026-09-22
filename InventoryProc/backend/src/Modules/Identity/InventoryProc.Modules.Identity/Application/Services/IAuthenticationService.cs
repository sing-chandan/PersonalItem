using InventoryProc.SharedKernel.Common;
using InventoryProc.Modules.Identity.Application.DTOs;

namespace InventoryProc.Modules.Identity.Application.Services;

public interface IAuthenticationService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken);
    Task<Result> LogoutAsync(Guid userId);
}
