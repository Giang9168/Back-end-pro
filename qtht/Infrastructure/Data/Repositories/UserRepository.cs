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
/// rồi map UserEntity (ORM) sang User (Domain) trước khi trả ra ngoài.
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
        var users = new EntityCollection<UserEntity>();
        await adapter.FetchQueryAsync(qf.User, users, cancellationToken);
        return users.Select(ToDomain).ToList();
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var entity = await adapter.FetchFirstAsync(qf.User.Where(UserFields.Id.Equal(id)), cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var entity = await adapter.FetchFirstAsync(
            qf.User.Where(UserFields.UserName.Equal(userName)), cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    /// <summary>Ranh giới ORM: kiểu LLBLGen dừng lại ở đây, không lọt ra ngoài Infrastructure.</summary>
    private static User ToDomain(UserEntity e) => new()
    {
        Id = e.Id,
        RoleId = e.RoleId,
        UserName = e.UserName
    };
}
