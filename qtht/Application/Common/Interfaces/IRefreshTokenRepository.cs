using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Lưu trữ refresh token. Mọi phương thức nhận/trả chuỗi THÔ —
/// việc băm trước khi ghi và khi tra cứu là chi tiết của Infrastructure.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Lưu token mới. Truyền chuỗi thô, repository tự băm.</summary>
    Task<RefreshToken> AddAsync(
        Guid userId, string rawToken, DateTime expiresAt, string? createdByIp,
        CancellationToken cancellationToken = default);

    /// <summary>Tra token theo chuỗi thô. Trả cả token đã thu hồi/hết hạn để phát hiện tái sử dụng.</summary>
    Task<RefreshToken?> FindByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default);

    Task RevokeAsync(
        Guid tokenId, string reason, string? revokedByIp, Guid? replacedBy,
        CancellationToken cancellationToken = default);

    /// <summary>Thu hồi toàn bộ token còn sống của user. Dùng khi phát hiện token bị đánh cắp.</summary>
    Task<int> RevokeAllActiveForUserAsync(
        Guid userId, string reason, string? revokedByIp,
        CancellationToken cancellationToken = default);
}
