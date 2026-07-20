namespace Domain.Entities;

/// <summary>
/// User nghiệp vụ — thuần POCO, không phụ thuộc ORM nào.
/// Infrastructure chịu trách nhiệm map từ UserEntity (LLBLGen) sang đây.
/// </summary>
public class User
{
    public required string Id { get; init; }
    public string? RoleId { get; init; }
    public string? UserName { get; init; }
}
