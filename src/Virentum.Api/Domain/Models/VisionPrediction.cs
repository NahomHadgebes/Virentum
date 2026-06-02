using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Models;

/// <summary>
/// The normalised output of the computer-vision stage, independent of any
/// particular vendor (Azure Custom Vision today, something else tomorrow).
/// Processors consume this immutable value object and never touch the raw
/// vendor payload.
/// </summary>
/// <param name="Fruit">The fruit the prediction relates to.</param>
/// <param name="RipenessScore">
/// A normalised ripeness signal in the inclusive range [0.0, 1.0], where 0 is
/// completely unripe and 1 is fully over-ripe / spoiled.
/// </param>
/// <param name="Tags">
/// The full set of model tag probabilities, keyed by tag name, for traceability
/// and richer fruit-specific reasoning.
/// </param>
public sealed record VisionPrediction(
    SupportedFruit Fruit,
    double RipenessScore,
    IReadOnlyDictionary<string, double> Tags);
