using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Strategy contract for turning a vendor-neutral <see cref="VisionPrediction"/>
/// into a fruit-specific commercial verdict.
///
/// Every fruit owns exactly one implementation of this interface, keeping its
/// ripeness thresholds, cold-chain advice and discount policy fully isolated.
/// Introducing a new fruit (e.g. Apple, Mango) means adding a new class that
/// implements this interface — no existing controller, service or factory code
/// is touched (Open/Closed Principle).
/// </summary>
public interface IFruitProcessor
{
    /// <summary>The single fruit this processor is responsible for.</summary>
    SupportedFruit Fruit { get; }

    /// <summary>
    /// The fruit's full ripeness policy, ascending and covering 0 through 100.
    /// <see cref="Assess"/> evaluates from this list, and the API publishes it,
    /// so the documented thresholds are the ones actually applied.
    /// </summary>
    IReadOnlyList<Models.RipenessBand> Bands { get; }

    /// <summary>The colours this fruit can legitimately present.</summary>
    Models.ColourProfile ColourProfile { get; }

    /// <summary>
    /// A message for the operator when the image is dominated by a colour this
    /// fruit never takes, or null when nothing contradicts the selection.
    /// </summary>
    string? DescribeColourMismatch(Models.VisionPrediction prediction);

    /// <summary>
    /// Evaluates a vision prediction and returns the merchandising assessment.
    /// </summary>
    RipenessAssessment Assess(VisionPrediction prediction);
}
