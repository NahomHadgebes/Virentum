using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

/// <summary>
/// The banana bands are Underripe to 42, ReadyForSale to 75, ActionRequired to
/// 88, Expired above. Each boundary is asserted from both sides, because that
/// is where an off-by-one between &lt; and &lt;= would hide.
/// </summary>
public sealed class BananaProcessorTests
{
    private static RipenessAssessment Assess(double score) =>
        new BananaProcessor().Assess(
            new VisionPrediction(SupportedFruit.Banana, score, new Dictionary<string, double>()),
            Audience.Consumer);

    [Fact]
    public void Declares_the_fruit_it_handles()
    {
        Assert.Equal(SupportedFruit.Banana, new BananaProcessor().Fruit);
    }

    [Theory]
    [InlineData(0.00, 0)]
    [InlineData(0.42, 42)]
    public void Is_underripe_up_to_and_including_42_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.Underripe, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.43, 43)]
    [InlineData(0.75, 75)]
    public void Is_ready_for_sale_from_43_through_75_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.ReadyForSale, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.76, 76)]
    [InlineData(0.88, 88)]
    public void Requires_action_from_76_through_88_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.ActionRequired, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.89, 89)]
    [InlineData(1.00, 100)]
    public void Is_expired_above_88_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.Expired, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Fact]
    public void Interpolates_the_measured_percent_into_the_discount_advice()
    {
        var assessment = Assess(0.81);

        Assert.Equal(81, assessment.RipenessPercent);
        Assert.Contains("81%", assessment.Recommendation, StringComparison.Ordinal);
        Assert.Contains("discount label", assessment.Recommendation, StringComparison.Ordinal);
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
