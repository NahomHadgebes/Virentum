using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// One ripeness stage as published to the client, with everything a guide needs
/// to show it: where it sits on the scale, what the fruit looks like there, what
/// it means commercially and for eating, and how to say it to either reader.
/// </summary>
/// <param name="MinPercent">First ripeness percent in the stage, inclusive.</param>
/// <param name="MaxPercent">Last ripeness percent in the stage, inclusive.</param>
/// <param name="StageName">What this stage is called for this fruit.</param>
/// <param name="Appearance">How the fruit looks and feels here.</param>
/// <param name="SwatchHex">Representative colour of the fruit at this stage.</param>
/// <param name="CommercialStatus">The merchandising decision for a store.</param>
/// <param name="Edibility">The answer for someone about to eat it.</param>
/// <param name="BusinessGuidance">
/// Advice for a store. Where it quotes the measured value it contains a
/// <c>{0}</c> placeholder — a template, not finished copy.
/// </param>
/// <param name="ConsumerGuidance">Advice for a person, on the same terms.</param>
public sealed record RipenessBandResponse(
    int MinPercent,
    int MaxPercent,
    string StageName,
    string Appearance,
    string SwatchHex,
    CommercialStatus CommercialStatus,
    EdibilityVerdict Edibility,
    string BusinessGuidance,
    string ConsumerGuidance);

/// <summary>
/// A fruit's complete ripeness policy. These are the very bands the processor
/// evaluates against, so the published stages cannot fall out of step with the
/// ones actually applied to a scan.
/// </summary>
public sealed record FruitProfileResponse(
    SupportedFruit FruitType,
    IReadOnlyList<RipenessBandResponse> Bands);
