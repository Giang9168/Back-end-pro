using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

// Command – đăng ký user mới
//
// LƯU Ý: chưa nhận Password/Email vì bảng "user" hiện chỉ có id/user_name/role_id.
// Cố tình KHÔNG nhận mật khẩu còn hơn nhận rồi âm thầm vứt đi — người dùng API
// sẽ tưởng mật khẩu đã được lưu. Bổ sung sau khi thêm cột (xem TODO ở RegisterHandler).
public record RegisterCommand : IRequest<Result<RegisterResponse>>
{
    public required string UserName { get; init; }
    public string? RoleId { get; init; }
}
