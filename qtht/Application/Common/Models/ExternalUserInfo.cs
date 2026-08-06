namespace Application.Common.Models;

/// <summary>
/// Danh tính đã được nhà cung cấp ngoài (Google, Facebook...) xác minh.
/// Mọi provider đều quy về cùng bộ thông tin này — handler không cần biết provider nào.
/// </summary>
public record ExternalUserInfo
{
    /// <summary>Mã provider, ví dụ "GOOGLE".</summary>
    public required string Provider { get; init; }

    /// <summary>Định danh vĩnh viễn của user bên phía provider (sub của Google).</summary>
    public required string ProviderUserId { get; init; }

    public string? Email { get; init; }

    /// <summary>Chỉ tin email để liên kết tài khoản khi provider xác nhận đã verify.</summary>
    public bool EmailVerified { get; init; }

    public string? DisplayName { get; init; }
}
