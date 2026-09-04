using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

/// <summary>
/// The pear bands are Underripe to 34, ReadyForSale to 66, ActionRequired to
/// 85, Expired above. Each boundary is asserted from both sides, because that
/// is where an off-by-one between &lt; and &lt;= would hide.
///
/// The band edges are lower than a banana's on purpose: a pear is sold hard and
/// ripens from the core outwards, so it is past its best sooner than its skin
/// suggests.
/// </summary>
public sealed class PearProcessorTests
{
    private static RipenessAssessment Assess(double score) =>
        new PearProcessor().Assess(
            new VisionPrediction(SupportedFruit.Pear, score, new Dictionary<string, double>()),
            Audience.Consumer);

    [Fact]
    public void Declares_the_fruit_it_handles()
    {
        Assert.Equal(SupportedFruit.Pear, new PearProcessor().Fruit);
    }

    [Theory]
    [InlineData(0.00, 0)]
    [InlineData(0.34, 34)]
    public void Is_underripe_up_to_and_including_34_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.Underripe, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.35, 35)]
    [InlineData(0.66, 66)]
    public void Is_ready_for_sale_from_35_through_66_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.ReadyForSale, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.67, 67)]
    [InlineData(0.85, 85)]
    public void Requires_action_from_67_through_85_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.ActionRequired, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.86, 86)]
    [InlineData(1.00, 100)]
    public void Is_expired_above_85_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.Expired, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Fact]
    public void Interpolates_the_measured_percent_into_the_discount_advice()
    {
        var assessment = new PearProcessor().Assess(
            new VisionPrediction(SupportedFruit.Pear, 0.72, new Dictionary<string, double>()),
            Audience.Business);

        Assert.Equal(72, assessment.RipenessPercent);
        Assert.Contains("72%", assessment.Recommendation, StringComparison.Ordinal);
    }

    /// <summary>
    /// A pear yields at the neck long before the belly softens, so the advice
    /// has to say which end to press or it repeats a mistake most people make.
    /// </summary>
    [Fact]
    public void Tells_the_reader_to_press_the_neck_rather_than_the_belly()
    {
        Assert.Contains("neck", Assess(0.50).Recommendation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Always_produces_advice()
    {
        foreach (var score in new[] { 0.1, 0.5, 0.8, 0.99 })
        {
            Assert.False(string.IsNullOrWhiteSpace(Assess(score).Recommendation));
        }
    }

    [Theory]
    [InlineData(-0.5, 0, CommercialStatus.Underripe)]
    [InlineData(1.5, 100, CommercialStatus.Expired)]
    public void Clamps_a_score_that_falls_outside_zero_to_one(
        double score,
        int expectedPercent,
        CommercialStatus expectedStatus)
    {
        var assessment = Assess(score);

        Assert.Equal(expectedPercent, assessment.RipenessPercent);
        Assert.Equal(expectedStatus, assessment.CommercialStatus);
    }
}
