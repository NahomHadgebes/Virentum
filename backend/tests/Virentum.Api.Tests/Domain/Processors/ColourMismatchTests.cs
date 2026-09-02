using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

/// <summary>
/// Virentum does not identify produce — the operator selects the fruit and the
/// vision stage only measures colour. These tests pin down the one contradiction
/// the colour data can actually catch, and, just as importantly, the ones it
/// cannot, so the check is never mistaken for classification.
/// </summary>
public sealed class ColourMismatchTests
{
    private static VisionPrediction Colours(
        SupportedFruit fruit,
        double green,
        double yellow,
        double brownDark) =>
        new(fruit, 0.5, new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            [ColourBuckets.Green] = green,
            [ColourBuckets.Yellow] = yellow,
            [ColourBuckets.BrownDark] = brownDark,
        });

    /// <summary>The case that prompted this: a photo of bananas, filed as avocado.</summary>
    [Fact]
    public void Flags_a_yellow_image_declared_as_an_avocado()
    {
        var mismatch = new AvocadoProcessor().DescribeColourMismatch(
            Colours(SupportedFruit.Avocado, green: 0.10, yellow: 0.80, brownDark: 0.10));

        Assert.NotNull(mismatch);
        Assert.Contains("80%", mismatch, StringComparison.Ordinal);
        Assert.Contains("yellow", mismatch, StringComparison.Ordinal);
        Assert.Contains("Avocado", mismatch, StringComparison.Ordinal);
        Assert.Contains("not fruit identity", mismatch, StringComparison.Ordinal);
    }

    [Fact]
    public void Accepts_a_green_image_declared_as_an_avocado()
    {
        Assert.Null(new AvocadoProcessor().DescribeColourMismatch(
            Colours(SupportedFruit.Avocado, green: 0.85, yellow: 0.05, brownDark: 0.10)));
    }

    [Fact]
    public void Accepts_a_dark_image_declared_as_an_avocado()
    {
        Assert.Null(new AvocadoProcessor().DescribeColourMismatch(
            Colours(SupportedFruit.Avocado, green: 0.20, yellow: 0.05, brownDark: 0.75)));
    }

    /// <summary>
    /// A little yellow is lighting or a highlight, not a different fruit. The
    /// threshold is deliberately blunt.
    /// </summary>
    [Fact]
    public void Tolerates_a_minority_of_off_profile_colour()
    {
        Assert.Null(new AvocadoProcessor().DescribeColourMismatch(
            Colours(SupportedFruit.Avocado, green: 0.55, yellow: 0.35, brownDark: 0.10)));
    }

    [Fact]
    public void Flags_only_once_the_off_profile_share_passes_half()
    {
        var processor = new AvocadoProcessor();

        Assert.Null(processor.DescribeColourMismatch(
            Colours(SupportedFruit.Avocado, green: 0.51, yellow: 0.49, brownDark: 0.0)));
        Assert.NotNull(processor.DescribeColourMismatch(
            Colours(SupportedFruit.Avocado, green: 0.49, yellow: 0.51, brownDark: 0.0)));
    }

    /// <summary>
    /// A banana passes through every measured bucket on its way from green to
    /// black, so nothing about its colour can contradict a banana. Asserting
    /// this keeps the check from being read as identification.
    /// </summary>
    [Theory]
    [InlineData(1.0, 0.0, 0.0)]
    [InlineData(0.0, 1.0, 0.0)]
    [InlineData(0.0, 0.0, 1.0)]
    public void Never_flags_a_banana_whatever_the_colour(double green, double yellow, double brownDark)
    {
        Assert.Null(new BananaProcessor().DescribeColourMismatch(
            Colours(SupportedFruit.Banana, green, yellow, brownDark)));
    }

    /// <summary>
    /// An avocado photo filed as a banana is not detectable this way — both are
    /// green or dark. The check catches contradictions, not identity, and this
    /// test documents that limit rather than hiding it.
    /// </summary>
    [Fact]
    public void Cannot_detect_an_avocado_photo_declared_as_a_banana()
    {
        Assert.Null(new BananaProcessor().DescribeColourMismatch(
            Colours(SupportedFruit.Banana, green: 0.2, yellow: 0.0, brownDark: 0.8)));
    }

    /// <summary>
    /// A provider that reports something other than colour says nothing about
    /// colour. The Custom Vision stub reports ripe/unripe, and silence is the
    /// honest answer there.
    /// </summary>
    [Fact]
    public void Stays_silent_when_the_provider_reported_no_colour_buckets()
    {
        var prediction = new VisionPrediction(
            SupportedFruit.Avocado,
            0.5,
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["ripe"] = 0.9,
                ["unripe"] = 0.1,
            });

        Assert.Null(new AvocadoProcessor().DescribeColourMismatch(prediction));
    }

    [Fact]
    public void Stays_silent_when_there_are_no_tags_at_all()
    {
        var prediction = new VisionPrediction(
            SupportedFruit.Avocado, 0.5, new Dictionary<string, double>());

        Assert.Null(new AvocadoProcessor().DescribeColourMismatch(prediction));
    }

    [Fact]
    public void Every_fruit_declares_a_colour_profile()
    {
        IFruitProcessor[] processors = { new BananaProcessor(), new AvocadoProcessor() };

        foreach (var processor in processors)
        {
            Assert.NotEmpty(processor.ColourProfile.PlausibleBuckets);
            Assert.All(
                processor.ColourProfile.PlausibleBuckets,
                bucket => Assert.Contains(bucket, ColourBuckets.All));
        }
    }
}
