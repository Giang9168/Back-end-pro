using Application.Common.Interfaces;
using Application.Common.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

/// <summary>Cấu hình Google Sign-In. ClientId không phải secret — để trong appsettings được.</summary>
public sealed record GoogleAuthSettings(string? ClientId);

/// <summary>
/// Xác minh ID token của Google (OIDC). Thư viện Google.Apis.Auth tự tải public key
/// của Google (có cache), kiểm chữ ký RS256, hạn và audience — không cần client secret.
/// </summary>
public sealed class GoogleAuthProvider : IExternalAuthProvider
{
    public const string Name = "GOOGLE";

    private readonly GoogleAuthSettings _settings;
    private readonly ILogger<GoogleAuthProvider> _logger;

    public GoogleAuthProvider(GoogleAuthSettings settings, ILogger<GoogleAuthProvider> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public string ProviderName => Name;

    public async Task<ExternalUserInfo?> ValidateAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientId))
        {
            _logger.LogWarning(
                "Google Sign-In chưa cấu hình. Điền 'Google:ClientId' trong appsettings.json của Web_API.");
            return null;
        }

        try
        {
            // Audience bắt buộc: token phát cho app khác (aud khác) sẽ bị loại tại đây
            var payload = await GoogleJsonWebSignature.ValidateAsync(token,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _settings.ClientId }
                });

            return new ExternalUserInfo
            {
                Provider       = Name,
                ProviderUserId = payload.Subject,
                Email          = payload.Email,
                EmailVerified  = payload.EmailVerified,
                DisplayName    = payload.Name
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning("ID token Google không hợp lệ: {Reason}", ex.Message);
            return null;
        }
    }
}
