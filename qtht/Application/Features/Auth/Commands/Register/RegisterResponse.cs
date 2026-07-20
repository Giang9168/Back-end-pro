namespace Application.Features.Auth.Commands.Register;

public record RegisterResponse
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public string? RoleId { get; init; }
}
