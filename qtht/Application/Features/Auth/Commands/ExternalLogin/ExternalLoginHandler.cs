using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.ExternalLogin;

public class ExternalLoginHandler : IRequestHandler<ExternalLoginCommand, Result<LoginResponse>>
{
    private const string DefaultRoleCode = "USER";
    private const string InvalidToken = "Đăng nhập không thành công. Vui lòng thử lại.";

    private readonly IEnumerable<IExternalAuthProvider> _providers;
    private readonly IUserRepository _users;
    private readonly IUserLoginRepository _userLogins;
    private readonly IRoleRepository _roles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenGenerator _tokens;
    private readonly IJwtSettingsProvider _jwtSettings;
    private readonly ILogger<ExternalLoginHandler> _logger;

    public ExternalLoginHandler(
        IEnumerable<IExternalAuthProvider> providers,
        IUserRepository users,
        IUserLoginRepository userLogins,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenGenerator tokens,
        IJwtSettingsProvider jwtSettings,
        ILogger<ExternalLoginHandler> logger)
    {
        _providers = providers;
        _users = users;
        _userLogins = userLogins;
        _roles = roles;
        _refreshTokens = refreshTokens;
        _tokens = tokens;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
        ExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        var provider = _providers.FirstOrDefault(p =>
            p.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            return Result<LoginResponse>.Failure(
                "Phương thức đăng nhập không được hỗ trợ", "AUTH_PROVIDER_NOT_SUPPORTED");
        }

        // Provider tự xác minh token (chữ ký, hạn, audience). Fail thì chỉ log,
        // ra ngoài trả thông báo chung — không tiết lộ lý do cho kẻ dò.
        var info = await provider.ValidateAsync(request.Token, cancellationToken);
        if (info is null)
        {
            return Result<LoginResponse>.Failure(InvalidToken, "AUTH_EXTERNAL_TOKEN_INVALID");
        }

        var user = await FindOrCreateUserAsync(info, cancellationToken);
        if (user is null)
        {
            return Result<LoginResponse>.Failure(
                $"Chưa có vai trò mặc định '{DefaultRoleCode}' trong hệ thống", "REGISTER_DEFAULT_ROLE_MISSING");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure("Tài khoản đã bị khóa", "AUTH_USER_INACTIVE");
        }

        var role = await _roles.GetByIdAsync(user.RoleId, cancellationToken);
        var roleCode = role?.Code ?? string.Empty;

        var accessToken = _tokens.CreateAccessToken(user, roleCode);

        var rawRefreshToken = _tokens.CreateRefreshToken();
        await _refreshTokens.AddAsync(
            user.Id,
            rawRefreshToken,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            request.ClientIp,
            cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            UserId       = user.Id,
            Username     = user.UserName,
            Email        = user.Email,
            Role         = roleCode,
            Token        = accessToken.Value,
            RefreshToken = rawRefreshToken,
            ExpiresAt    = accessToken.ExpiresAt
        });
    }

    /// <summary>
    /// Trả về user ứng với danh tính ngoài, theo thứ tự:
    /// đã liên kết → liên kết vào tài khoản trùng email (chỉ khi email verified) → tạo mới.
    /// Trả về null duy nhất khi thiếu role mặc định.
    /// </summary>
    private async Task<User?> FindOrCreateUserAsync(ExternalUserInfo info, CancellationToken ct)
    {
        var userId = await _userLogins.FindUserIdByLoginAsync(info.Provider, info.ProviderUserId, ct);
        if (userId is { } id)
        {
            return await _users.GetByIdAsync(id, ct);
        }

        // Chỉ tự liên kết theo email khi provider xác nhận email đã verify —
        // không thì kẻ xấu khai email người khác ở provider là chiếm được tài khoản.
        if (info.EmailVerified && !string.IsNullOrWhiteSpace(info.Email))
        {
            var existing = await _users.GetByEmailAsync(info.Email, ct);
            if (existing is not null)
            {
                await _userLogins.AddAsync(existing.Id, info.Provider, info.ProviderUserId, ct);
                _logger.LogInformation(
                    "Liên kết {Provider} vào tài khoản có sẵn. UserId={UserId}", info.Provider, existing.Id);
                return existing;
            }
        }

        var role = await _roles.GetByCodeAsync(DefaultRoleCode, ct);
        if (role is null)
        {
            return null;
        }

        var user = new User
        {
            Id             = Guid.NewGuid(),
            UserName       = await GenerateUserNameAsync(info, ct),
            Email          = info.Email,
            PasswordHash   = null,               // không có mật khẩu — đăng nhập qua provider
            RoleId         = role.Id,
            IsActive       = true,
            EmailConfirmed = info.EmailVerified
        };

        var created = await _users.AddAsync(user, ct);
        await _userLogins.AddAsync(created.Id, info.Provider, info.ProviderUserId, ct);

        _logger.LogInformation(
            "Tạo tài khoản mới qua {Provider}. UserId={UserId}, UserName={UserName}",
            info.Provider, created.Id, created.UserName);

        return created;
    }

    /// <summary>
    /// Sinh username từ phần trước @ của email (lọc ký tự lạ), tránh trùng
    /// bằng cách nối thêm số — DB có unique index trên user_name.
    /// </summary>
    private async Task<string> GenerateUserNameAsync(ExternalUserInfo info, CancellationToken ct)
    {
        var baseName = (info.Email?.Split('@')[0] ?? info.Provider.ToLowerInvariant())
            .ToLowerInvariant();
        baseName = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-').ToArray());
        if (baseName.Length < 3)
        {
            baseName = $"{info.Provider.ToLowerInvariant()}user";
        }

        var candidate = baseName;
        for (var suffix = 1; await _users.GetByUserNameAsync(candidate, ct) is not null; suffix++)
        {
            candidate = $"{baseName}{suffix}";
        }

        return candidate;
    }
}
