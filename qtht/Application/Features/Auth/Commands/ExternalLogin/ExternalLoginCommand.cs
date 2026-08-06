using Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using MediatR;

namespace Application.Features.Auth.Commands.ExternalLogin;

/// <summary>
/// Đăng nhập/đăng ký qua nhà cung cấp ngoài. Một endpoint lo cả hai:
/// chưa có tài khoản thì tạo, có rồi thì đăng nhập.
/// </summary>
public record ExternalLoginCommand : IRequest<Result<LoginResponse>>
{
    /// <summary>Mã provider client chọn, ví dụ "GOOGLE".</summary>
    public required string Provider { get; init; }

    /// <summary>Token do provider phát cho client (ID token của Google).</summary>
    public required string Token { get; init; }

    /// <summary>IP thật do server ghi đè — không tin giá trị client gửi lên.</summary>
    public string? ClientIp { get; init; }
}
