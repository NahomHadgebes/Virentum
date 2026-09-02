using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Models;

/// <summary>
/// The verdict produced by an <see cref="Processors.IFruitProcessor"/>, carrying
/// both readings of the same measurement: what a store should do, and whether a
/// person can still eat it. Immutable so it can cross layers without copies.
/// </summary>
/// <param name="RipenessPercent">Ripeness as a whole percentage [0, 100].</param>
/// <param name="CommercialStatus">The merchandising decision.</param>
/// <param name="Edibility">The answer for someone about to eat it.</param>
/// <param name="StageName">What this stage is called for this fruit.</param>
/// <param name="Appearance">How the fruit looks here, so the reader can check.</param>
/// <param name="Recommendation">Advice written for the requested audience.</param>
/// <param name="Factors">What the measurement rested on, in checkable terms.</param>
public sealed record RipenessAssessment(
    int RipenessPercent,
    CommercialStatus CommercialStatus,
    EdibilityVerdict Edibility,
    string StageName,
    string Appearance,
    string Recommendation,
    IReadOnlyList<AnalysisFactor> Factors);
