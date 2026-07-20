namespace Application.Common.Interfaces;

/// <summary>
/// Chỉ lộ ra những thông số Application thật sự cần (thời hạn refresh token).
/// Khoá ký, issuer, audience là chuyện của Infrastructure — không lộ lên đây.
/// </summary>
public interface IJwtSettingsProvider
{
    int RefreshTokenDays { get; }
}
