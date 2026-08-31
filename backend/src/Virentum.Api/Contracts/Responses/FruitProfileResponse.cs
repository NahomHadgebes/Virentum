using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// One ripeness band as published to the client.
/// </summary>
/// <param name="GuidanceTemplate">
/// The advice for this band. Where it quotes the measured value it contains a
/// <c>{0}</c> placeholder — this is a template, not finished copy, and a client
/// rendering the catalogue must present that placeholder deliberately rather
/// than printing it raw.
/// </param>
public sealed record RipenessBandResponse(
    int MinPercent,
    int MaxPercent,
    CommercialStatus CommercialStatus,
    string GuidanceTemplate);

/// <summary>
/// A fruit's complete ripeness policy. These are the very bands the processor
/// evaluates against, so the published thresholds cannot fall out of step with
/// the ones actually applied to a scan.
/// </summary>
public sealed record FruitProfileResponse(
    SupportedFruit FruitType,
    IReadOnlyList<RipenessBandResponse> Bands);
