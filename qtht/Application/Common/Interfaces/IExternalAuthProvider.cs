using Application.Common.Models;

namespace Application.Common.Interfaces;

/// <summary>
/// Một nhà cung cấp đăng nhập ngoài. Mỗi provider (Google, Facebook...) một hiện thực;
/// khác nhau ở cách xác minh token, giống nhau ở kết quả trả về (ExternalUserInfo).
/// </summary>
public interface IExternalAuthProvider
{
    /// <summary>Mã định danh provider, so khớp với trường Provider client gửi lên.</summary>
    string ProviderName { get; }

    /// <summary>
    /// Xác minh token do client đưa lên. Trả về null nếu token không hợp lệ —
    /// lý do cụ thể chỉ ghi log, không lộ ra ngoài.
    /// </summary>
    Task<ExternalUserInfo?> ValidateAsync(string token, CancellationToken cancellationToken = default);
}
