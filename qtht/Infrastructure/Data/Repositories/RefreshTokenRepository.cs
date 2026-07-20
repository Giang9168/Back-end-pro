using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Domain.Entities;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using Qtht.Data.EntityClasses;
using Qtht.Data.FactoryClasses;
using Qtht.Data.HelperClasses;

namespace Infrastructure.Data.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDataAccessAdapterFactory _adapterFactory;

    public RefreshTokenRepository(IDataAccessAdapterFactory adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    public async Task<RefreshToken> AddAsync(
        Guid userId, string rawToken, DateTime expiresAt, string? createdByIp,
        CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();

        var entity = new RefreshTokenEntity
        {
            Id          = Guid.NewGuid(),
            UserId      = userId,
            TokenHash   = Hash(rawToken),
            ExpiresAt   = expiresAt,
            CreatedAt   = DateTime.UtcNow,
            CreatedByIp = createdByIp,
            IsNew       = true
        };

        await adapter.SaveEntityAsync(entity, cancellationToken);
        return ToDomain(entity);
    }

    public async Task<RefreshToken?> FindByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();
        var entity = await adapter.FetchFirstAsync(
            qf.RefreshToken.Where(RefreshTokenFields.TokenHash.Equal(Hash(rawToken))), cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task RevokeAsync(
        Guid tokenId, string reason, string? revokedByIp, Guid? replacedBy,
        CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();

        var entity = await adapter.FetchFirstAsync(
            qf.RefreshToken.Where(RefreshTokenFields.Id.Equal(tokenId)), cancellationToken);
        if (entity is null) return;

        entity.RevokedAt     = DateTime.UtcNow;
        entity.RevokedReason = reason;
        entity.RevokedByIp   = revokedByIp;
        entity.ReplacedBy    = replacedBy;

        await adapter.SaveEntityAsync(entity, cancellationToken);
    }

    public async Task<int> RevokeAllActiveForUserAsync(
        Guid userId, string reason, string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        using var adapter = _adapterFactory.Create();
        var qf = new QueryFactory();

        var tokens = new EntityCollection<RefreshTokenEntity>();
        await adapter.FetchQueryAsync(
            qf.RefreshToken.Where(RefreshTokenFields.UserId.Equal(userId)
                .And(RefreshTokenFields.RevokedAt.IsNull())),
            tokens, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var t in tokens)
        {
            t.RevokedAt     = now;
            t.RevokedReason = reason;
            t.RevokedByIp   = revokedByIp;
        }

        if (tokens.Count > 0)
        {
            await adapter.SaveEntityCollectionAsync(tokens, false, false, cancellationToken);
        }

        return tokens.Count;
    }

    /// <summary>
    /// SHA-256 của chuỗi thô. Refresh token nhạy cảm ngang mật khẩu — ai đọc được
    /// bảng là chiếm được phiên. Không dùng BCrypt vì token vốn đã ngẫu nhiên 64 byte,
    /// không có gì để brute-force; SHA-256 đủ và nhanh hơn nhiều.
    /// </summary>
    private static string Hash(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static RefreshToken ToDomain(RefreshTokenEntity e) => new()
    {
        Id            = e.Id,
        UserId        = e.UserId,
        ExpiresAt     = e.ExpiresAt,
        CreatedAt     = e.CreatedAt,
        CreatedByIp   = e.CreatedByIp,
        RevokedAt     = e.RevokedAt,
        RevokedByIp   = e.RevokedByIp,
        RevokedReason = e.RevokedReason,
        ReplacedBy    = e.ReplacedBy
    };
}
