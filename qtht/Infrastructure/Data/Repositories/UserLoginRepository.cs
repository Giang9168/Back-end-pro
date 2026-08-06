using Application.Common.Interfaces;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using Qtht.Data.EntityClasses;
using Qtht.Data.FactoryClasses;
using Qtht.Data.HelperClasses;

namespace Infrastructure.Data.Repositories;

public sealed class UserLoginRepository : IUserLoginRepository
{
    private readonly IDataAccessAdapterFactory _adapterFactory;

    public UserLoginRepository(IDataAccessAdapterFactory adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    public async Task<Guid?> FindUserIdByLoginAsync(
        string provider, string providerUserId, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var entity = await adapter.FetchFirstAsync(
            qf.UserLogin.Where(
                UserLoginFields.Provider.Equal(provider)
                    .And(UserLoginFields.ProviderUserId.Equal(providerUserId))),
            cancellationToken);

        return entity?.UserId;
    }

    public async Task AddAsync(
        Guid userId, string provider, string providerUserId, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();

        var entity = new UserLoginEntity
        {
            UserId         = userId,
            Provider       = provider,
            ProviderUserId = providerUserId,
            CreatedAt      = DateTime.UtcNow,
            IsNew          = true
        };

        await adapter.SaveEntityAsync(entity, cancellationToken);
    }
}
