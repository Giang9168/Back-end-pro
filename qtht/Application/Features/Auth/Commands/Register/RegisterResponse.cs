namespace Application.Features.Auth.Commands.Register;

// Không có PasswordHash — DTO là cái van chặn field nhạy cảm lọt ra API
public record RegisterResponse
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public string? Email { get; init; }
    public required Guid RoleId { get; init; }
    public required string RoleCode { get; init; }
}
