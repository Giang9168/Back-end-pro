using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private const string InvalidToken = "Refresh token không hợp lệ hoặc đã hết hạn";

    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenGenerator _tokens;
    private readonly IJwtSettingsProvider _jwtSettings;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IUserRepository users,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenGenerator tokens,
        IJwtSettingsProvider jwtSettings,
        ILogger<RefreshTokenHandler> logger)
    {
        _users = users;
        _roles = roles;
        _refreshTokens = refreshTokens;
        _tokens = tokens;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var stored = await _refreshTokens.FindByRawTokenAsync(request.RefreshToken, cancellationToken);

        if (stored is null)
        {
            return Result<LoginResponse>.Failure(InvalidToken, "AUTH_INVALID_REFRESH_TOKEN");
        }

        // ── PHÁT HIỆN TÁI SỬ DỤNG ────────────────────────────────────────
        // Token đã bị thu hồi mà vẫn có người trình ra: hoặc client giữ bản cũ,
        // hoặc token đã bị đánh cắp. Không phân biệt được, nên xử lý theo hướng
        // an toàn nhất — thu hồi TOÀN BỘ token còn sống của user, buộc đăng nhập lại.
        if (stored.IsRevoked)
        {
            var count = await _refreshTokens.RevokeAllActiveForUserAsync(
                stored.UserId, RevokeReason.ReuseDetected, request.ClientIp, cancellationToken);

            _logger.LogWarning(
                "Phát hiện tái sử dụng refresh token đã thu hồi. UserId={UserId}, TokenId={TokenId}, IP={Ip}, đã thu hồi {Count} token",
                stored.UserId, stored.Id, request.ClientIp, count);

            return Result<LoginResponse>.Failure(InvalidToken, "AUTH_REFRESH_TOKEN_REUSED");
        }

        if (stored.IsExpired)
        {
            return Result<LoginResponse>.Failure(InvalidToken, "AUTH_REFRESH_TOKEN_EXPIRED");
        }

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            // Tài khoản bị khoá/xoá sau khi token được cấp
            await _refreshTokens.RevokeAsync(
                stored.Id, RevokeReason.Admin, request.ClientIp, null, cancellationToken);
            return Result<LoginResponse>.Failure(InvalidToken, "AUTH_USER_INACTIVE");
        }

        var role = await _roles.GetByIdAsync(user.RoleId, cancellationToken);
        var roleCode = role?.Code ?? string.Empty;

        // ── XOAY VÒNG ────────────────────────────────────────────────────
        // Cấp token mới rồi mới thu hồi token cũ, đồng thời nối chuỗi qua ReplacedBy.
        // Mỗi refresh token chỉ dùng được ĐÚNG MỘT LẦN.
        var rawNewToken = _tokens.CreateRefreshToken();
        var newToken = await _refreshTokens.AddAsync(
            user.Id,
            rawNewToken,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            request.ClientIp,
            cancellationToken);

        await _refreshTokens.RevokeAsync(
            stored.Id, RevokeReason.Rotated, request.ClientIp, newToken.Id, cancellationToken);

        var accessToken = _tokens.CreateAccessToken(user, roleCode);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            UserId       = user.Id,
            Username     = user.UserName,
            Email        = user.Email,
            Role         = roleCode,
            Token        = accessToken.Value,
            RefreshToken = rawNewToken,
            ExpiresAt    = accessToken.ExpiresAt
        });
    }
}
