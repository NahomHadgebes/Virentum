using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Banana-specific merchandising logic. Bananas brown quickly once past peak,
/// so the "action required" window is wide and the advice is display-oriented.
/// All thresholds and copy live here and nowhere else.
/// </summary>
public sealed class BananaProcessor : IFruitProcessor
{
    // Ripeness band boundaries, as whole percentages, tuned for bananas.
    // Bananas progress green → yellow → spotted → black, so the sweet spot is the
    // middle: too green is unripe (hold), too dark is spoiled (remove).
    private const int UnderripeCeiling = 42; // <= 42% ⇒ green, not ready
    private const int ReadyCeiling = 75;     // 43–75% ⇒ prime shelf
    private const int ActionCeiling = 88;    // 76–88% ⇒ discount fast

    public SupportedFruit Fruit => SupportedFruit.Banana;

    public RipenessAssessment Assess(VisionPrediction prediction)
    {
        var ripenessPercent = ToPercent(prediction.RipenessScore);

        return ripenessPercent switch
        {
            <= UnderripeCeiling => new RipenessAssessment(
                ripenessPercent,
                CommercialStatus.Underripe,
                "Still green and underripe. Hold off the shelf and allow 1–3 days " +
                "to ripen before display."),

            <= ReadyCeiling => new RipenessAssessment(
                ripenessPercent,
                CommercialStatus.ReadyForSale,
                "Perfect condition. Suitable for prime display at the front shelf."),

            <= ActionCeiling => new RipenessAssessment(
                ripenessPercent,
                CommercialStatus.ActionRequired,
                $"This batch is {ripenessPercent}% ripe. Print a 50% discount label " +
                "immediately to ensure sale within 24 hours."),

            _ => new RipenessAssessment(
                ripenessPercent,
                CommercialStatus.Expired,
                "Quality below shelf threshold. Remove from display and route to " +
                "compost / supplier credit log."),
        };
    }

    private static int ToPercent(double score) =>
        (int)Math.Round(Math.Clamp(score, 0d, 1d) * 100d);
}
