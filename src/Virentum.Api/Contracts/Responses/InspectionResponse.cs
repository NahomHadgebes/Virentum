using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// The inspection result returned to the frontend. Immutable record matching the
/// client contract exactly: <c>fruitType</c>, <c>ripenessPercent</c>,
/// <c>commercialStatus</c>, <c>recommendation</c>, <c>scannedAt</c>.
///
/// Enums serialise to their string names (via a global
/// <c>JsonStringEnumConverter</c>), e.g. "Banana" / "ReadyForSale".
/// </summary>
public sealed record InspectionResponse(
    SupportedFruit FruitType,
    int RipenessPercent,
    CommercialStatus CommercialStatus,
    string Recommendation,
    DateTimeOffset ScannedAt);
