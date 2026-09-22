using InventoryProc.SharedKernel.Common;
using InventoryProc.SharedKernel.Exceptions;
using InventoryProc.Modules.Identity.Domain.Entities;
using InventoryProc.Modules.Identity.Domain.Enums;
using InventoryProc.Modules.Identity.Application.DTOs;
using InventoryProc.Modules.Tenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace InventoryProc.Modules.Identity.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly DbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(DbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // Find user by email (case-insensitive) - ignore tenant filter during login
        var user = await _context.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null)
            return Result<AuthResponse>.Fail("Invalid email or password");

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Fail("Invalid email or password");

        // Check if user is active
        if (!user.IsActive)
            return Result<AuthResponse>.Fail("User account is deactivated");

        // Generate tokens
        var (token, expiresAt) = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));
        await _context.SaveChangesAsync();

        var response = new AuthResponse
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };

        return Result<AuthResponse>.Ok(response, "Login successful");
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists globally - ignore tenant filter
        var existingUser = await _context.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser)
            return Result<AuthResponse>.Fail("Email already registered");

        // Generate unique tenant code from tenant name
        var baseCode = request.TenantName.ToUpper()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("_", "");

        if (baseCode.Length > 10)
            baseCode = baseCode.Substring(0, 10);

        var tenantCode = baseCode;
        var counter = 1;

        while (await _context.Set<Tenant>().IgnoreQueryFilters().AnyAsync(t => t.Code == tenantCode))
        {
            tenantCode = $"{baseCode}{counter}";
            counter++;
        }

        // Create new tenant
        var tenant = new Tenant(tenantCode, request.TenantName, request.TenantName, request.Email);
        _context.Set<Tenant>().Add(tenant);
        await _context.SaveChangesAsync();

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // First user in new tenant is always Admin
        var role = UserRole.Admin;

        var user = new User(
            tenant.Id,
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            role
        );

        if (!string.IsNullOrWhiteSpace(request.Mobile))
            user.UpdateProfile(request.FirstName, request.LastName, request.Mobile);

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        var (token, expiresAt) = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));
        await _context.SaveChangesAsync();

        var response = new AuthResponse
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };

        return Result<AuthResponse>.Ok(response, "Registration successful");
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result<AuthResponse>.Fail("Refresh token is required");

        var user = await _context.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null)
            return Result<AuthResponse>.Fail("Invalid refresh token");

        if (!user.IsActive)
            return Result<AuthResponse>.Fail("User account is deactivated");

        if (!user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime.Value < DateTime.UtcNow)
            return Result<AuthResponse>.Fail("Refresh token expired");

        // Generate new tokens
        var (token, expiresAt) = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));
        await _context.SaveChangesAsync();

        var response = new AuthResponse
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            Token = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt
        };

        return Result<AuthResponse>.Ok(response, "Token refreshed successfully");
    }

    public async Task<Result> LogoutAsync(Guid userId)
    {
        var user = await _context.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return Result.Fail("User not found");

        user.ClearRefreshToken();
        await _context.SaveChangesAsync();

        return Result.Ok("Logout successful");
    }

    private (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("tenantId", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
