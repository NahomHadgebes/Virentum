using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Infrastructure.Persistence.Repositories;

namespace Virentum.Api.Tests.TestDoubles;

/// <summary>Captures what the service tried to persist, in order.</summary>
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
            Saved.Where(record => record.StoreId == storeId).Take(limit).ToList());
}
