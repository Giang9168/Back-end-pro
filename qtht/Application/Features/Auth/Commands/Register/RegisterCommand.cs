using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

// Command – đăng ký user mới
public record RegisterCommand : IRequest<Result<RegisterResponse>>
{
    public required string UserName { get; init; }

    /// <summary>Mật khẩu thô. Chỉ tồn tại trong bộ nhớ, được băm ngay ở handler.</summary>
    public required string Password { get; init; }

    public string? Email { get; init; }

    /// <summary>Bỏ trống thì gán vai trò USER.</summary>
    public Guid? RoleId { get; init; }
}
