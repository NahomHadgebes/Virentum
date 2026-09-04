using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

/// <summary>
/// The colour stage counts hues; on a poor photograph the number it produces is
/// arithmetic rather than evidence. These tests hold it to saying so, because a
/// weak reading presented as a clean verdict is the failure mode that matters.
/// </summary>
public sealed class EvidenceTests
{
    private static VisionPrediction Reading(
        SupportedFruit fruit,
        double green,
        double yellow,
        double brownDark,
        double? analysedShare) =>
        new(fruit, 0.5, new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            [ColourBuckets.Green] = green,
            [ColourBuckets.Yellow] = yellow,
            [ColourBuckets.BrownDark] = brownDark,
        }, analysedShare);

    [Fact]
    public void A_clear_photograph_of_the_right_fruit_raises_nothing()
    {
        var evidence = new AvocadoProcessor().AssessEvidence(
            Reading(SupportedFruit.Avocado, 0.7, 0.05, 0.25, analysedShare: 0.75));

        Assert.True(evidence.IsReliable);
        Assert.Empty(evidence.Concerns);
    }

    /// <summary>
    /// A picture that is mostly worktop or packaging is not a measurement of
    /// produce, however confident the arithmetic looks.
    /// </summary>
    [Fact]
    public void Flags_a_frame_that_is_mostly_background()
    {
        var evidence = new AvocadoProcessor().AssessEvidence(
            Reading(SupportedFruit.Avocado, 0.7, 0.05, 0.25, analysedShare: 0.08));

        Assert.False(evidence.IsReliable);
        Assert.Contains(evidence.Concerns, c => c.Contains("8%", StringComparison.Ordinal));
        Assert.Contains(evidence.Concerns, c => c.Contains("Fill more of the frame", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0.19, false)]
    [InlineData(0.20, true)]
    public void Twenty_percent_of_the_frame_is_the_dividing_line(double share, bool reliable)
    {
        var evidence = new AvocadoProcessor().AssessEvidence(
            Reading(SupportedFruit.Avocado, 0.7, 0.05, 0.25, analysedShare: share));

        Assert.Equal(reliable, evidence.IsReliable);
    }

    /// <summary>
    /// The case that started this: a cut avocado photographed flesh-up. Pale
    /// flesh reads yellow, which says nothing about an avocado's ripeness.
    /// </summary>
    [Fact]
    public void Flags_a_colour_that_carries_no_meaning_for_the_selected_fruit()
    {
        var evidence = new AvocadoProcessor().AssessEvidence(
            Reading(SupportedFruit.Avocado, 0.15, 0.70, 0.15, analysedShare: 0.6));

        Assert.False(evidence.IsReliable);
        Assert.Contains(evidence.Concerns, c => c.Contains("no ripeness meaning", StringComparison.Ordinal));
        Assert.Contains(evidence.Concerns, c => c.Contains("photograph the skin", StringComparison.Ordinal));
    }

    [Fact]
    public void Raises_both_concerns_when_both_apply()
    {
        var evidence = new AvocadoProcessor().AssessEvidence(
            Reading(SupportedFruit.Avocado, 0.15, 0.70, 0.15, analysedShare: 0.05));

        Assert.Equal(2, evidence.Concerns.Count);
    }

    /// <summary>
    /// A banana legitimately shows every measured colour, so only coverage can
    /// ever limit its reading. Stating this keeps the check from being read as
    /// fruit identification.
    /// </summary>
    [Fact]
    public void A_banana_is_only_ever_limited_by_coverage()
    {
        var processor = new BananaProcessor();

        Assert.True(processor.AssessEvidence(
            Reading(SupportedFruit.Banana, 0.0, 1.0, 0.0, analysedShare: 0.6)).IsReliable);
        Assert.False(processor.AssessEvidence(
            Reading(SupportedFruit.Banana, 0.0, 1.0, 0.0, analysedShare: 0.05)).IsReliable);
    }

    /// <summary>
    /// A provider that cannot report coverage must not be treated as if it had
    /// reported none. Absence of a figure is not a figure of zero.
    /// </summary>
    [Fact]
    public void Says_nothing_about_coverage_when_the_provider_did_not_report_it()
    {
        var evidence = new AvocadoProcessor().AssessEvidence(
            Reading(SupportedFruit.Avocado, 0.7, 0.05, 0.25, analysedShare: null));

        Assert.True(evidence.IsReliable);
    }

    /// <summary>
    /// An image with no produce-like colour at all scores 0.5 by default. That
    /// is the absence of a measurement, and it has to arrive labelled as one.
    /// </summary>
    [Fact]
    public void Flags_an_image_with_no_produce_like_colour_at_all()
    {
        var nothing = new VisionPrediction(
            SupportedFruit.Avocado,
            0.5,
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase),
            AnalysedShare: 0d);

        Assert.False(new AvocadoProcessor().AssessEvidence(nothing).IsReliable);
    }
}
