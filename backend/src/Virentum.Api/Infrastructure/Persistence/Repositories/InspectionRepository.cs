using Microsoft.EntityFrameworkCore;
using Virentum.Api.Infrastructure.Persistence.Entities;

namespace Virentum.Api.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IInspectionRepository"/>.</summary>
public sealed class InspectionRepository : IInspectionRepository
{
    private readonly VirentumDbContext _db;

    public InspectionRepository(VirentumDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(InspectionRecord record, CancellationToken cancellationToken = default)
    {
        await _db.Inspections.AddAsync(record, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InspectionRecord>> GetRecentByStoreAsync(
        string storeId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await _db.Inspections
            .AsNoTracking()
            .Where(i => i.StoreId == storeId)
            .OrderByDescending(i => i.ScannedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
