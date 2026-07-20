namespace Application.Features.Auth.Commands.Login;

// Không có PasswordHash — DTO là cái van chặn field nhạy cảm lọt ra API
public record LoginResponse
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }

    /// <summary>Mã vai trò (ADMIN, USER), không phải Id.</summary>
    public required string Role { get; init; }

    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
}
