using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// The inspection result returned to the frontend.
///
/// Both readings of the same measurement travel together: what a store should do
/// with the stock, and whether a person can still eat it. They genuinely differ —
/// produce a shop must pull from display is often fine at home — and a client
/// showing only one of them is answering only one of the two questions.
///
/// Enums serialise to their string names, e.g. "Banana" / "ReadyForSale".
/// </summary>
/// <param name="FruitType">The fruit the operator selected, echoed back.</param>
/// <param name="Audience">The audience this wording was written for.</param>
/// <param name="RipenessPercent">Measured ripeness as a whole percent, 0-100.</param>
/// <param name="StageName">What this stage is called for this fruit.</param>
/// <param name="Appearance">How the fruit looks here, so the reader can check.</param>
/// <param name="CommercialStatus">The merchandising decision.</param>
/// <param name="Edibility">The answer for someone about to eat it.</param>
/// <param name="Recommendation">Advice written for the requested audience.</param>
/// <param name="Factors">
/// What the measurement rested on, largest first, in terms the reader can check
/// against their own photographs.
/// </param>
/// <param name="ImageCount">How many photographs the reading pools.</param>
/// <param name="ScannedAt">When the inspection was recorded.</param>
/// <param name="Evidence">
/// How much weight this reading can carry. The assessment is always returned;
/// this says whether it should be taken at face value, and if not, why.
/// </param>
public sealed record InspectionResponse(
    SupportedFruit FruitType,
    Audience Audience,
    int RipenessPercent,
    string StageName,
    string Appearance,
    CommercialStatus CommercialStatus,
    EdibilityVerdict Edibility,
    string Recommendation,
    IReadOnlyList<AnalysisFactorResponse> Factors,
    int ImageCount,
    DateTimeOffset ScannedAt,
    InspectionEvidenceResponse Evidence);

/// <summary>
/// One measured contribution to the assessment, in terms the reader can check
/// against the photograph they took.
/// </summary>
/// <param name="Label">The colour, written for a human.</param>
/// <param name="Share">Its share of everything the analysis could classify, 0-1.</param>
/// <param name="Meaning">What that colour indicates for this particular fruit.</param>
public sealed record AnalysisFactorResponse(string Label, double Share, string Meaning);

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
