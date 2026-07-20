using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private const string InvalidCredentials = "Tài khoản hoặc mật khẩu không đúng";

    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokens;
    private readonly IJwtSettingsProvider _jwtSettings;

    public LoginHandler(
        IUserRepository users,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher,
        IJwtTokenGenerator tokens,
        IJwtSettingsProvider jwtSettings)
    {
        _users = users;
        _roles = roles;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
        _tokens = tokens;
        _jwtSettings = jwtSettings;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByUserNameAsync(request.EmailOrUsername, cancellationToken);

        // Mọi nhánh thất bại trả CÙNG một thông báo. Nói rõ "sai mật khẩu" là vô tình
        // xác nhận tài khoản có tồn tại — giúp kẻ tấn công dò danh sách user.
        if (user is null || !user.IsActive)
        {
            return Result<LoginResponse>.Failure(InvalidCredentials, "AUTH_INVALID_CREDENTIALS");
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(InvalidCredentials, "AUTH_INVALID_CREDENTIALS");
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
            RefreshToken = rawRefreshToken,   // bản thô, chỉ lộ ra đúng lần này
            ExpiresAt    = accessToken.ExpiresAt
        });
    }
}
