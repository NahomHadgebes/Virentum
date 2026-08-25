using Microsoft.EntityFrameworkCore;
using Virentum.Api.Infrastructure.Persistence.Entities;

namespace Virentum.Api.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public sealed class UserRepository : IUserRepository
{
    private readonly VirentumDbContext _db;

    public UserRepository(VirentumDbContext db)
    {
        _db = db;
    }

    public async Task<UserAccount?> FindByStoreIdAsync(
        string storeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.StoreId == storeId, cancellationToken);
    }
}
