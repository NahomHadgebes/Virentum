using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Avocado-specific merchandising logic. Avocados are sold firm and rely on the
/// cold chain, so the advice references temperature and the "ready" band runs
/// higher than for bananas. Thresholds and copy are isolated to this class.
/// </summary>
public sealed class AvocadoProcessor : IFruitProcessor
{
    // Ripeness band boundaries, as whole percentages, tuned for avocados.
    private const int ReadyCeiling = 82;   // <= 82% ⇒ premium produce section
    private const int ActionCeiling = 93;  // 83–93% ⇒ discount / sell today

    public SupportedFruit Fruit => SupportedFruit.Avocado;

    public RipenessAssessment Assess(VisionPrediction prediction)
    {
        var ripenessPercent = ToPercent(prediction.RipenessScore);

        return ripenessPercent switch
        {
            <= ReadyCeiling => new RipenessAssessment(
                ripenessPercent,
                CommercialStatus.ReadyForSale,
                "Firm and ready for the premium produce section. Maintain at 4°C."),

            <= ActionCeiling => new RipenessAssessment(
                ripenessPercent,
                CommercialStatus.ActionRequired,
                $"Ripening fast at {ripenessPercent}%. Apply a 50% discount label and " +
                "move to the 'ready to eat' basket for same-day sale."),

            _ => new RipenessAssessment(
                ripenessPercent,
                CommercialStatus.Expired,
                "Overripe and bruising. Remove from display and route to compost / " +
                "supplier credit log."),
        };
    }

    private static int ToPercent(double score) =>
        (int)Math.Round(Math.Clamp(score, 0d, 1d) * 100d);
}
