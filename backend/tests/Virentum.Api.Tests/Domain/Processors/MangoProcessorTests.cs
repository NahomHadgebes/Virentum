using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

/// <summary>
/// The mango bands are Underripe to 32, ReadyForSale to 64, ActionRequired to
/// 86, Expired above. Each boundary is asserted from both sides, because that
/// is where an off-by-one between &lt; and &lt;= would hide.
/// </summary>
public sealed class MangoProcessorTests
{
    private static RipenessAssessment Assess(double score, Audience audience = Audience.Consumer) =>
        new MangoProcessor().Assess(
            new VisionPrediction(SupportedFruit.Mango, score, new Dictionary<string, double>()),
            audience);

    [Fact]
    public void Declares_the_fruit_it_handles()
    {
        Assert.Equal(SupportedFruit.Mango, new MangoProcessor().Fruit);
    }

    [Theory]
    [InlineData(0.00, 0)]
    [InlineData(0.32, 32)]
    public void Is_underripe_up_to_and_including_32_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.Underripe, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.33, 33)]
    [InlineData(0.64, 64)]
    public void Is_ready_for_sale_from_33_through_64_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.ReadyForSale, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.65, 65)]
    [InlineData(0.86, 86)]
    public void Requires_action_from_65_through_86_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.ActionRequired, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Theory]
    [InlineData(0.87, 87)]
    [InlineData(1.00, 100)]
    public void Is_expired_above_86_percent(double score, int expectedPercent)
    {
        var assessment = Assess(score);

        Assert.Equal(CommercialStatus.Expired, assessment.CommercialStatus);
        Assert.Equal(expectedPercent, assessment.RipenessPercent);
    }

    [Fact]
    public void Interpolates_the_measured_percent_into_the_discount_advice()
    {
        var assessment = Assess(0.70, Audience.Business);

        Assert.Equal(70, assessment.RipenessPercent);
        Assert.Contains("70%", assessment.Recommendation, StringComparison.Ordinal);
    }

    /// <summary>
    /// The colour stage cannot read a red blush, and a blush is sun exposure
    /// rather than ripeness anyway. The advice says so instead of letting a
    /// reader grade by the thing the picture makes most obvious.
    /// </summary>
    [Fact]
    public void Warns_a_shop_not_to_grade_a_mango_by_its_blush()
    {
        Assert.Contains("blush", Assess(0.50, Audience.Business).Recommendation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Points_a_shopper_at_smell_and_give_rather_than_colour()
    {
        var recommendation = Assess(0.50).Recommendation;

        Assert.Contains("smell", recommendation, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("squeeze", recommendation, StringComparison.OrdinalIgnoreCase);
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
