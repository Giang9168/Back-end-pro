using System.Text.Json.Serialization;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

// Command – đăng nhập, trả về JWT + refresh token
public record LoginCommand : IRequest<Result<LoginResponse>>
{
    public required string EmailOrUsername { get; init; }
    public required string Password { get; init; }

    /// <summary>
    /// IP người gọi, dùng để ghi vết token. [JsonIgnore] để client KHÔNG tự khai được —
    /// controller ghi đè bằng địa chỉ thật lấy từ HttpContext.
    /// </summary>
    [JsonIgnore]
    public string? ClientIp { get; init; }
}
