using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// The inspection result returned to the frontend.
///
/// Enums serialise to their string names (via a global
/// <c>JsonStringEnumConverter</c>), e.g. "Banana" / "ReadyForSale".
/// </summary>
/// <param name="FruitType">The fruit the operator selected, echoed back.</param>
/// <param name="RipenessPercent">Measured ripeness as a whole percent, 0-100.</param>
/// <param name="CommercialStatus">The merchandising decision for that ripeness.</param>
/// <param name="Recommendation">Advice for the store associate.</param>
/// <param name="ScannedAt">When the inspection was recorded.</param>
/// <param name="ColourMismatch">
/// Set when the image is dominated by a colour the selected fruit never takes,
/// and null otherwise. The assessment is still returned: Virentum measures
/// colour rather than identifying produce, so this is a prompt to check the
/// selection, not a rejection of the scan.
/// </param>
public sealed record InspectionResponse(
    SupportedFruit FruitType,
    int RipenessPercent,
    CommercialStatus CommercialStatus,
    string Recommendation,
    DateTimeOffset ScannedAt,
    string? ColourMismatch);
