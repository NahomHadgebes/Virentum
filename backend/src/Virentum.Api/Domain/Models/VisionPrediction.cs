using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Models;

/// <summary>
/// The normalised output of the computer-vision stage, independent of any
/// particular vendor. Processors consume this immutable value object and never
/// touch the raw vendor payload.
/// </summary>
/// <param name="Fruit">The fruit the prediction relates to.</param>
/// <param name="RipenessScore">
/// A normalised ripeness signal in [0.0, 1.0], where 0 is completely unripe and
/// 1 is fully over-ripe.
/// </param>
/// <param name="Tags">
/// The full set of model tag probabilities, keyed by tag name, for traceability
/// and richer fruit-specific reasoning.
/// </param>
/// <param name="AnalysedShare">
/// How much of the supplied imagery produced a usable reading, 0 to 1, or null
/// when the provider cannot say. A score derived from a sliver of the frame is
/// not worth the same as one derived from most of it, and the difference has to
/// travel with the score rather than be assumed away.
/// </param>
/// <param name="ImageCount">
/// How many photographs the reading pools. More angles is more evidence, and the
/// assessment says so rather than leaving the reader to guess.
/// </param>
public sealed record VisionPrediction(
    SupportedFruit Fruit,
    double RipenessScore,
    IReadOnlyDictionary<string, double> Tags,
    double? AnalysedShare = null,
    int ImageCount = 1);
