using System.Globalization;
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
    /// <summary>
    /// How much of the measured colour has to sit outside the fruit's profile
    /// before the selection is worth questioning. Half is deliberately blunt:
    /// the check exists to catch a picture of a different fruit, not to
    /// second-guess unusual lighting.
    /// </summary>
    private const double MismatchThreshold = 0.5;

    protected FruitProcessor(
        SupportedFruit fruit,
        IReadOnlyList<RipenessBand> bands,
        ColourProfile colourProfile)
    {
        Fruit = fruit;
        Bands = Validate(fruit, bands);
        ColourProfile = colourProfile;
    }

    public SupportedFruit Fruit { get; }

    public IReadOnlyList<RipenessBand> Bands { get; }

    public ColourProfile ColourProfile { get; }

    /// <summary>
    /// Below this share of the frame, the reading rests on too little of the
    /// picture to stand on its own. A photograph that is mostly worktop or
    /// packaging is not a measurement of produce.
    /// </summary>
    private const double MinimumAnalysedShare = 0.20;

    public InspectionEvidence AssessEvidence(VisionPrediction prediction)
    {
        var concerns = new List<string>();

        if (prediction.AnalysedShare is { } analysed && analysed < MinimumAnalysedShare)
        {
            var share = (analysed * 100d).ToString("F0", CultureInfo.InvariantCulture);
            concerns.Add(
                $"Only {share}% of this image held produce-like colour; the rest read as " +
                "background. Fill more of the frame with the fruit and scan again.");
        }

        var mismatch = DescribeColourMismatch(prediction);
        if (mismatch is not null)
        {
            concerns.Add(mismatch);
        }

        return InspectionEvidence.From(concerns);
    }

    /// <summary>
    /// The image is dominated by a colour this fruit never takes, so either the
    /// wrong fruit is selected or the photograph is not of the fruit's skin —
    /// which is the surface the colour stage is a proxy for.
    /// </summary>
    private string? DescribeColourMismatch(VisionPrediction prediction)
    {
        // Only colour buckets are evidence here. A provider that reports
        // something else — the Custom Vision stub reports ripe/unripe — says
        // nothing about colour, and silence is the honest answer.
        var measured = prediction.Tags
            .Where(tag => ColourBuckets.All.Contains(tag.Key))
            .ToList();

        if (measured.Count == 0)
        {
            return null;
        }

        var offProfile = measured
            .Where(tag => !ColourProfile.PlausibleBuckets.Contains(tag.Key))
            .ToList();

        if (offProfile.Count == 0)
        {
            return null;
        }

        var share = offProfile.Sum(tag => tag.Value);
        if (share < MismatchThreshold)
        {
            return null;
        }

        var dominant = offProfile.OrderByDescending(tag => tag.Value).First();
        var percent = (share * 100d).ToString("F0", CultureInfo.InvariantCulture);

        return $"{percent}% of this image reads as {ColourBuckets.Describe(dominant.Key)}, " +
               $"which carries no ripeness meaning for {Fruit}. Virentum measures the colour " +
               "of a fruit's skin, not its identity - check the selected fruit, and " +
               "photograph the skin rather than cut flesh.";
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
