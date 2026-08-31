using Microsoft.EntityFrameworkCore;
using Virentum.Api.Domain.Models;
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

    public async Task<InspectionStatistics> GetStatisticsSinceAsync(
        string storeId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        var scans = _db.Inspections
            .AsNoTracking()
            .Where(i => i.StoreId == storeId && i.ScannedAt >= since);

        var total = await scans.CountAsync(cancellationToken);
        if (total == 0)
        {
            // Averaging or taking the maximum of an empty set would have to be
            // guarded anyway; returning early says the same thing more plainly.
            return InspectionStatistics.Empty;
        }

        var byStatus = await scans
            .GroupBy(i => i.CommercialStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var byFruit = await scans
            .GroupBy(i => i.FruitType)
            .Select(group => new { Fruit = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var averageRipeness = await scans.AverageAsync(i => (double)i.RipenessPercent, cancellationToken);
        var lastScanAt = await scans.MaxAsync(i => i.ScannedAt, cancellationToken);

        return new InspectionStatistics(
            total,
            byStatus.ToDictionary(entry => entry.Status, entry => entry.Count),
            byFruit.ToDictionary(entry => entry.Fruit, entry => entry.Count),
            averageRipeness,
            lastScanAt);
    }
}
