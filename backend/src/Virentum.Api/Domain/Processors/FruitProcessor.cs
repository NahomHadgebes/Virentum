using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Shared evaluation for every fruit. A concrete processor supplies only the
/// fruit it handles and its bands; selecting the band for a score is identical
/// for all of them and lives here rather than being copied per fruit.
///
/// The bands are validated on construction — that is, at startup, since
/// processors are registered as singletons — so a fruit added with a gap or an
/// overlap in its bands fails immediately instead of returning a wrong verdict
/// for scores that fall in the hole.
/// </summary>
public abstract class FruitProcessor : IFruitProcessor
{
    protected FruitProcessor(SupportedFruit fruit, IReadOnlyList<RipenessBand> bands)
    {
        Fruit = fruit;
        Bands = Validate(fruit, bands);
    }

    public SupportedFruit Fruit { get; }

    public IReadOnlyList<RipenessBand> Bands { get; }

    public RipenessAssessment Assess(VisionPrediction prediction)
    {
        var ripenessPercent = ToPercent(prediction.RipenessScore);
        var band = Bands.First(candidate => ripenessPercent <= candidate.MaxPercent);

        return new RipenessAssessment(
            ripenessPercent,
            band.CommercialStatus,
            band.DescribeFor(ripenessPercent));
    }

    /// <summary>A vision score is normalised [0, 1]; ripeness is a whole percent.</summary>
    private static int ToPercent(double score) =>
        (int)Math.Round(Math.Clamp(score, 0d, 1d) * 100d);

    /// <summary>
    /// Bands must cover 0 through 100 exactly once, in ascending order, so that
    /// every possible score resolves to exactly one decision.
    /// </summary>
    private static IReadOnlyList<RipenessBand> Validate(
        SupportedFruit fruit,
        IReadOnlyList<RipenessBand> bands)
    {
        ArgumentNullException.ThrowIfNull(bands);

        if (bands.Count == 0)
        {
            throw new ArgumentException($"'{fruit}' declares no ripeness bands.", nameof(bands));
        }

        var expectedMin = 0;
        foreach (var band in bands)
        {
            if (band.MinPercent != expectedMin)
            {
                throw new ArgumentException(
                    $"'{fruit}' has a gap or overlap in its ripeness bands: expected the next " +
                    $"band to start at {expectedMin} but it starts at {band.MinPercent}.",
                    nameof(bands));
            }

            if (band.MaxPercent < band.MinPercent)
            {
                throw new ArgumentException(
                    $"'{fruit}' has a band ending at {band.MaxPercent} before it starts " +
                    $"at {band.MinPercent}.",
                    nameof(bands));
            }

            expectedMin = band.MaxPercent + 1;
        }

        if (expectedMin != 101)
        {
            throw new ArgumentException(
                $"'{fruit}' bands stop at {expectedMin - 1}%; they must cover through 100%.",
                nameof(bands));
        }

        return bands;
    }
}
