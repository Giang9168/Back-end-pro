using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

// Command – đăng nhập, trả về token
public record LoginCommand : IRequest<Result<LoginResponse>>
{
    public required string EmailOrUsername { get; init; }
    public required string Password { get; init; }
}
