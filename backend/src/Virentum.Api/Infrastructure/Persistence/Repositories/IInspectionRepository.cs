using Virentum.Api.Domain.Models;
using Virentum.Api.Infrastructure.Persistence.Entities;

namespace Virentum.Api.Infrastructure.Persistence.Repositories;

/// <summary>
/// Abstracts persistence of inspection records so business services depend on an
/// interface, not on EF Core directly.
/// </summary>
public interface IInspectionRepository
{
    Task AddAsync(InspectionRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InspectionRecord>> GetRecentByStoreAsync(
        string storeId,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates a store's activity from <paramref name="since"/> onwards.
    /// </summary>
    Task<InspectionStatistics> GetStatisticsSinceAsync(
        string storeId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);
}
