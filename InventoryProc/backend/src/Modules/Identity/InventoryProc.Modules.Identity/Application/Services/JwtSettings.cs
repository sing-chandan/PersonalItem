namespace InventoryProc.Modules.Identity.Application.Services;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 1440; // 24 hours
    public int RefreshTokenExpirationDays { get; set; } = 30;
}
