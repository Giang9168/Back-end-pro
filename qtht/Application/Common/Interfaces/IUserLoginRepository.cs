namespace Application.Common.Interfaces;

/// <summary>
/// Liên kết giữa app_user và tài khoản bên provider ngoài (bảng user_login).
/// Một user có thể có nhiều liên kết: mật khẩu + Google + Facebook cùng lúc.
/// </summary>
public interface IUserLoginRepository
{
    Task<Guid?> FindUserIdByLoginAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);

    Task AddAsync(Guid userId, string provider, string providerUserId, CancellationToken cancellationToken = default);
}
