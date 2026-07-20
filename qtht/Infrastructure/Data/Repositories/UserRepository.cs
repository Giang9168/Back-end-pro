using Application.Common.Interfaces;
using Domain.Entities;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using Qtht.Data.EntityClasses;
using Qtht.Data.FactoryClasses;
using Qtht.Data.HelperClasses;

namespace Infrastructure.Data.Repositories;

/// <summary>
/// Repository dùng LLBLGen: lấy adapter từ factory, query bằng QueryFactory,
/// rồi map AppUserEntity (ORM) sang User (Domain) trước khi trả ra ngoài.
/// Mỗi thao tác tạo adapter mới và dispose ngay (adapter ôm 1 connection).
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly IDataAccessAdapterFactory _adapterFactory;

    public UserRepository(IDataAccessAdapterFactory adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var users = new EntityCollection<AppUserEntity>();
        await adapter.FetchQueryAsync(qf.AppUser, users, cancellationToken);
        return users.Select(ToDomain).ToList();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var entity = await adapter.FetchFirstAsync(
            qf.AppUser.Where(AppUserFields.Id.Equal(id)), cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        // DB có unique index trên lower(user_name) nên so sánh cũng hạ về chữ thường
        var entity = await adapter.FetchFirstAsync(
            qf.AppUser.Where(AppUserFields.UserName.ToLower().Equal(userName.ToLowerInvariant())),
            cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();

        var entity = new AppUserEntity
        {
            Id             = user.Id,
            UserName       = user.UserName,
            Email          = user.Email,
            PasswordHash   = user.PasswordHash,
            RoleId         = user.RoleId,
            IsActive       = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt      = DateTime.UtcNow,
            IsNew          = true
        };

        await adapter.SaveEntityAsync(entity, cancellationToken);
        return ToDomain(entity);
    }

    /// <summary>Ranh giới ORM: kiểu LLBLGen dừng lại ở đây, không lọt ra ngoài Infrastructure.</summary>
    private static User ToDomain(AppUserEntity e) => new()
    {
        Id                = e.Id,
        UserName          = e.UserName,
        Email             = e.Email,
        PasswordHash      = e.PasswordHash,
        RoleId            = e.RoleId,
        IsActive          = e.IsActive,
        EmailConfirmed    = e.EmailConfirmed,
        AccessFailedCount = e.AccessFailedCount,
        LockoutEnd        = e.LockoutEnd,
        LastLoginAt       = e.LastLoginAt,
        CreatedAt         = e.CreatedAt
    };
}
