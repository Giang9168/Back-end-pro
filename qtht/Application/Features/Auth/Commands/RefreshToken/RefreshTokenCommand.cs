using System.Text.Json.Serialization;
using Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

// Command – đổi refresh token lấy cặp token mới
public record RefreshTokenCommand : IRequest<Result<LoginResponse>>
{
    public required string RefreshToken { get; init; }

    /// <summary>Controller ghi đè bằng IP thật, client không tự khai được.</summary>
    [JsonIgnore]
    public string? ClientIp { get; init; }
}
