using System.Text.Json.Serialization;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

// Command – thu hồi refresh token (đăng xuất)
public record LogoutCommand : IRequest<Result>
{
    public required string RefreshToken { get; init; }

    /// <summary>Thu hồi mọi phiên của user, không chỉ phiên hiện tại ("đăng xuất khỏi mọi thiết bị").</summary>
    public bool AllDevices { get; init; }

    [JsonIgnore]
    public string? ClientIp { get; init; }
}
