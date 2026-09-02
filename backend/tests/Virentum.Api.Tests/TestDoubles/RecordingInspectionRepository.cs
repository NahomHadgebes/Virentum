using Virentum.Api.Domain.Models;
using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Infrastructure.Persistence.Repositories;

namespace Virentum.Api.Tests.TestDoubles;

/// <summary>
/// Captures what the service tried to persist, and answers queries from the same
/// list. The aggregation mirrors what the EF implementation asks the database
/// for, so a service test can assert on shaping and zero-filling without a
/// database in the loop.
/// </summary>
internal sealed class RecordingInspectionRepository : IInspectionRepository
{
    public List<InspectionRecord> Saved { get; } = new();

    public Task AddAsync(InspectionRecord record, CancellationToken cancellationToken = default)
    {
        Saved.Add(record);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<InspectionRecord>> GetRecentByStoreAsync(
        string storeId,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<InspectionRecord>>(
            Saved.Where(record => record.StoreId == storeId)
                .OrderByDescending(record => record.ScannedAt)
                .Take(limit)
                .ToList());

    public Task<InspectionStatistics> GetStatisticsSinceAsync(
        string storeId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        var scans = Saved
            .Where(record => record.StoreId == storeId && record.ScannedAt >= since)
            .ToList();

        if (scans.Count == 0)
        {
            return Task.FromResult(InspectionStatistics.Empty);
        }

        return Task.FromResult(new InspectionStatistics(
            scans.Count,
            scans.GroupBy(record => record.CommercialStatus)
                .ToDictionary(group => group.Key, group => group.Count()),
            scans.GroupBy(record => record.FruitType)
                .ToDictionary(group => group.Key, group => group.Count()),
            scans.Average(record => (double)record.RipenessPercent),
            scans.Max(record => record.ScannedAt)));
    }
}
