using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens;

    public LogoutHandler(IRefreshTokenRepository refreshTokens)
    {
        _refreshTokens = refreshTokens;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var stored = await _refreshTokens.FindByRawTokenAsync(request.RefreshToken, cancellationToken);

        // Token không tồn tại / đã thu hồi → vẫn trả thành công.
        // Đăng xuất là thao tác idempotent, và báo lỗi ở đây chỉ giúp kẻ tấn công
        // biết token nào có thật.
        if (stored is null || stored.IsRevoked)
        {
            return Result.Success();
        }

        if (request.AllDevices)
        {
            await _refreshTokens.RevokeAllActiveForUserAsync(
                stored.UserId, RevokeReason.Logout, request.ClientIp, cancellationToken);
        }
        else
        {
            await _refreshTokens.RevokeAsync(
                stored.Id, RevokeReason.Logout, request.ClientIp, null, cancellationToken);
        }

        return Result.Success();
    }
}
