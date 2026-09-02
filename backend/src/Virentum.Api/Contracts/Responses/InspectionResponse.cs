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
/// <param name="Evidence">
/// How much weight this reading can carry. The assessment is always returned;
/// this says whether it should be taken at face value, and if not, why. A
/// client must not present an unreliable reading as a finding.
/// </param>
public sealed record InspectionResponse(
    SupportedFruit FruitType,
    int RipenessPercent,
    CommercialStatus CommercialStatus,
    string Recommendation,
    DateTimeOffset ScannedAt,
    InspectionEvidenceResponse Evidence);

/// <summary>
/// What the colour stage had to work with. The stage is a heuristic that counts
/// hues; on a poor photograph the number it produces is arithmetic rather than
/// evidence, and saying so is the difference between a measurement and a guess
/// wearing a measurement's clothes.
/// </summary>
/// <param name="IsReliable">False when at least one concern was raised.</param>
/// <param name="Concerns">Plain statements of what limits this reading.</param>
public sealed record InspectionEvidenceResponse(
    bool IsReliable,
    IReadOnlyList<string> Concerns);
