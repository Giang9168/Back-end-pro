namespace Domain.Entities;

/// <summary>Vai trò người dùng — ADMIN, USER...</summary>
public class Role
{
    public required Guid Id { get; init; }

    /// <summary>Mã bất biến dùng trong code (ADMIN, USER). Không dùng Name để so sánh.</summary>
    public required string Code { get; init; }

    public required string Name { get; init; }
    public string? Description { get; init; }
}
