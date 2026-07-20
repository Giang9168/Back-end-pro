namespace Domain.Entities;

/// <summary>
/// User nghiệp vụ — thuần POCO, không phụ thuộc ORM nào.
/// Infrastructure chịu trách nhiệm map từ AppUserEntity (LLBLGen) sang đây.
/// </summary>
public class User
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public string? Email { get; init; }

    /// <summary>Chuỗi băm BCrypt. Không bao giờ lộ ra DTO/API.</summary>
    public required string PasswordHash { get; init; }

    public required Guid RoleId { get; init; }

    public bool IsActive { get; init; } = true;
    public bool EmailConfirmed { get; init; }

    // Chống dò mật khẩu
    public int AccessFailedCount { get; init; }
    public DateTime? LockoutEnd { get; init; }

    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
