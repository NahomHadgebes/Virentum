using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Models;

/// <summary>
/// Aggregated inspection activity for one store over a time window. Counts are
/// produced by the database rather than by loading rows into memory, so the
/// query cost does not grow with how much a store has scanned.
/// </summary>
/// <param name="TotalScans">Inspections recorded in the window.</param>
/// <param name="CountByStatus">Scans per commercial status; only statuses that occurred.</param>
/// <param name="CountByFruit">Scans per fruit; only fruits that occurred.</param>
/// <param name="AverageRipenessPercent">Mean ripeness, or null when nothing was scanned.</param>
/// <param name="LastScanAt">Most recent scan, or null when nothing was scanned.</param>
public sealed record InspectionStatistics(
    int TotalScans,
    IReadOnlyDictionary<CommercialStatus, int> CountByStatus,
    IReadOnlyDictionary<SupportedFruit, int> CountByFruit,
    double? AverageRipenessPercent,
    DateTimeOffset? LastScanAt)
{
    /// <summary>A window in which nothing was scanned.</summary>
    public static InspectionStatistics Empty { get; } = new(
        0,
        new Dictionary<CommercialStatus, int>(),
        new Dictionary<SupportedFruit, int>(),
        null,
        null);
}
