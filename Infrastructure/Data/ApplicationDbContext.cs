using Application.Common.Interfaces;

namespace Infrastructure.Data;

public class ApplicationDbContext : IApplicationDbContext
{
    // TODO: Inherit from DbContext when Entity Framework Core is added
    // Example: public class ApplicationDbContext : DbContext, IApplicationDbContext

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Placeholder implementation
        return Task.FromResult(0);
    }
}
