namespace Domain.Entities;

/// <summary>
/// Refresh token đã cấp cho một user. Chuỗi thô KHÔNG được lưu ở đây —
/// DB chỉ giữ hash, y như mật khẩu.
/// </summary>
public class RefreshToken
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedByIp { get; init; }

    public DateTime? RevokedAt { get; init; }
    public string? RevokedByIp { get; init; }
    public string? RevokedReason { get; init; }

    /// <summary>Token nào đã thay thế token này (chuỗi xoay vòng).</summary>
    public Guid? ReplacedBy { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;
}

/// <summary>Lý do thu hồi — dùng chuỗi cố định để tra log cho dễ.</summary>
public static class RevokeReason
{
    public const string Logout        = "LOGOUT";
    public const string Rotated       = "ROTATED";
    public const string ReuseDetected = "REUSE_DETECTED";
    public const string Admin         = "ADMIN";
}
