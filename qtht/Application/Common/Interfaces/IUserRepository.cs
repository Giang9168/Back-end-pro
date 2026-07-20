using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Repository đọc/ghi User. Tầng Application định nghĩa hợp đồng bằng kiểu Domain,
/// Infrastructure hiện thực và tự lo phần map từ ORM.
/// </summary>
public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
}
