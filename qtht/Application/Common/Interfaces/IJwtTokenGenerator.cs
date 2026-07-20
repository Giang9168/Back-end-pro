using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>Access token đã sinh, kèm thời điểm hết hạn để trả về cho client.</summary>
public record AccessToken(string Value, DateTime ExpiresAt);

/// <summary>
/// Sinh token. Application chỉ biết hợp đồng — thuật toán ký, khoá bí mật,
/// thời hạn đều nằm ở Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    AccessToken CreateAccessToken(User user, string roleCode);

    /// <summary>
    /// Sinh refresh token ngẫu nhiên mã hoá an toàn (bản thô, trả cho client).
    /// Bản lưu trong DB là hash của chuỗi này.
    /// </summary>
    string CreateRefreshToken();
}
