namespace Infrastructure.Security;

/// <summary>
/// Nạp từ section "Jwt". SecretKey phải để trong user-secrets (dev) hoặc
/// biến môi trường Jwt__SecretKey (prod) — KHÔNG bao giờ commit vào appsettings.json.
/// </summary>
public sealed class JwtSettings : Application.Common.Interfaces.IJwtSettingsProvider
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "qtht-api";
    public string Audience { get; init; } = "qtht-client";
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>Access token nên ngắn — bị lộ cũng chỉ dùng được trong thời gian này.</summary>
    public int AccessTokenMinutes { get; init; } = 15;

    /// <summary>Refresh token dài hơn, nhưng thu hồi được vì có lưu trong DB.</summary>
    public int RefreshTokenDays { get; init; } = 7;
}
