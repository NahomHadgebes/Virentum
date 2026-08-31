using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Responses;

/// <summary>Scans of one commercial status within the window.</summary>
public sealed record StatusCount(CommercialStatus CommercialStatus, int Count);

/// <summary>Scans of one fruit within the window.</summary>
public sealed record FruitCount(SupportedFruit FruitType, int Count);

/// <summary>
/// A store's inspection activity over a rolling window.
/// </summary>
/// <param name="WindowDays">Length of the window the client asked for.</param>
/// <param name="Since">Start of the window, resolved on the server.</param>
/// <param name="TotalScans">Inspections recorded in the window.</param>
/// <param name="ByStatus">
/// Every <see cref="CommercialStatus"/> in declaration order, zero-filled where
/// nothing was scanned. A client charting this then has a stable set of
/// categories even for a quiet week, and never has to invent the missing ones.
/// </param>
/// <param name="ByFruit">
/// Every <see cref="SupportedFruit"/> in declaration order, zero-filled on the
/// same principle as <paramref name="ByStatus"/>.
/// </param>
/// <param name="AverageRipenessPercent">
/// Null when nothing was scanned — not zero, which would read as "completely
/// unripe" rather than "no data".
/// </param>
/// <param name="LastScanAt">Most recent scan in the window, or null.</param>
public sealed record InspectionSummaryResponse(
    int WindowDays,
    DateTimeOffset Since,
    int TotalScans,
    IReadOnlyList<StatusCount> ByStatus,
    IReadOnlyList<FruitCount> ByFruit,
    double? AverageRipenessPercent,
    DateTimeOffset? LastScanAt);
