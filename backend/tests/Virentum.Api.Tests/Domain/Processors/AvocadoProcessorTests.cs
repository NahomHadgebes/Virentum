using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

/// <summary>
/// Avocado bands run higher than banana: Underripe to 35, ReadyForSale to 82,
/// ActionRequired to 93, Expired above. That the two differ is the point of
/// having a processor per fruit, so both are pinned down separately.
/// </summary>
public sealed class AvocadoProcessorTests
{
    private static RipenessAssessment Assess(double score) =>
        new AvocadoProcessor().Assess(
            new VisionPrediction(SupportedFruit.Avocado, score, new Dictionary<string, double>()));

    [Fact]
    public void Declares_the_fruit_it_handles()
    {
        Assert.Equal(SupportedFruit.Avocado, new AvocadoProcessor().Fruit);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(0.35)]
    public void Is_underripe_up_to_and_including_35_percent(double score)
    {
        Assert.Equal(CommercialStatus.Underripe, Assess(score).CommercialStatus);
    }

    [Theory]
    [InlineData(0.36)]
    [InlineData(0.82)]
    public void Is_ready_for_sale_from_36_through_82_percent(double score)
    {
        Assert.Equal(CommercialStatus.ReadyForSale, Assess(score).CommercialStatus);
    }

    [Theory]
    [InlineData(0.83)]
    [InlineData(0.93)]
    public void Requires_action_from_83_through_93_percent(double score)
    {
        Assert.Equal(CommercialStatus.ActionRequired, Assess(score).CommercialStatus);
    }

    [Theory]
    [InlineData(0.94)]
    [InlineData(1.00)]
    public void Is_expired_above_93_percent(double score)
    {
        Assert.Equal(CommercialStatus.Expired, Assess(score).CommercialStatus);
    }

    /// <summary>
    /// 60% is prime shelf for an avocado but the same score would still be
    /// ReadyForSale for a banana; 80% separates them, and that is the
    /// fruit-specific behaviour worth protecting.
    /// </summary>
    [Fact]
    public void Differs_from_banana_where_the_bands_diverge()
    {
        var prediction = new VisionPrediction(
            SupportedFruit.Avocado, 0.80, new Dictionary<string, double>());

        Assert.Equal(CommercialStatus.ReadyForSale, new AvocadoProcessor().Assess(prediction).CommercialStatus);
        Assert.Equal(CommercialStatus.ActionRequired, new BananaProcessor().Assess(prediction).CommercialStatus);
    }

    [Fact]
    public void Interpolates_the_measured_percent_into_the_discount_advice()
    {
        var assessment = Assess(0.90);

        Assert.Equal(90, assessment.RipenessPercent);
        Assert.Contains("90%", assessment.Recommendation, StringComparison.Ordinal);
    }
}
