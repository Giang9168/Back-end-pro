using Application.Common.Interfaces;
using Domain.Entities;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using Qtht.Data.EntityClasses;
using Qtht.Data.FactoryClasses;
using Qtht.Data.HelperClasses;

namespace Infrastructure.Data.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IDataAccessAdapterFactory _adapterFactory;

    public RoleRepository(IDataAccessAdapterFactory adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var entity = await adapter.FetchFirstAsync(
            qf.AppRole.Where(AppRoleFields.Code.Equal(code)), cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var entity = await adapter.FetchFirstAsync(
            qf.AppRole.Where(AppRoleFields.Id.Equal(id)), cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    private static Role ToDomain(AppRoleEntity e) => new()
    {
        Id          = e.Id,
        Code        = e.Code,
        Name        = e.Name,
        Description = e.Description
    };
}
