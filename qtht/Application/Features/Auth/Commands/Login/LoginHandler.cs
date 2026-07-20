using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _users;

    public LoginHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Tra cứu user thật trong PostgreSQL qua LLBLGen
        var user = await _users.GetByUserNameAsync(request.EmailOrUsername, cancellationToken);

        if (user is null)
        {
            // Không nói rõ "sai user" hay "sai mật khẩu" để tránh dò tài khoản
            return Result<LoginResponse>.Failure("Tài khoản hoặc mật khẩu không đúng", "AUTH_INVALID_CREDENTIALS");
        }

        // ────────────────────────────────────────────────────────────────
        // CHƯA XÁC THỰC MẬT KHẨU — bảng "user" hiện chỉ có id/user_name/role_id,
        // không có cột lưu hash mật khẩu nên không thể kiểm tra.
        // Endpoint này CHƯA AN TOÀN, chỉ dùng để chạy thử luồng ở máy dev.
        // Muốn hoàn thiện, xem phần TODO ở cuối file.
        // ────────────────────────────────────────────────────────────────

        var dto = new LoginResponse
        {
            UserId       = user.Id,
            Username     = user.UserName ?? string.Empty,
            Role         = user.RoleId ?? string.Empty,
            Email        = string.Empty,   // bảng chưa có cột email
            Token        = string.Empty,   // chưa sinh JWT
            RefreshToken = string.Empty,
            ExpiresAt    = default
        };

        return Result<LoginResponse>.Success(dto);
    }
}

// TODO để login dùng được thật:
//  1. Thêm cột vào bảng "user": password_hash (text), email (text)
//  2. Sync catalog + F7 trong LLBLGen, bổ sung field vào Domain.Entities.User
//  3. Kiểm tra mật khẩu bằng BCrypt.Net-Next hoặc ASP.NET Core PasswordHasher
//     (KHÔNG tự viết hàm băm, KHÔNG lưu mật khẩu dạng thô)
//  4. Sinh JWT bằng Microsoft.AspNetCore.Authentication.JwtBearer,
//     đăng ký AddAuthentication + UseAuthentication trong Program.cs
