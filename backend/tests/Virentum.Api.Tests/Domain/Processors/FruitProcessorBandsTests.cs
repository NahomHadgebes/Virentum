using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

/// <summary>
/// The bands are now the single source of the thresholds — the processor
/// evaluates from them and the API publishes them. Since they are data, they can
/// be malformed, so the base class validates them at construction. Processors are
/// DI singletons, which means a bad set fails at startup rather than returning a
/// silently wrong verdict for scores that fall in the gap.
/// </summary>
public sealed class FruitProcessorBandsTests
{
    private sealed class TestProcessor : FruitProcessor
    {
        public TestProcessor(params RipenessBand[] bands)
            : base(SupportedFruit.Banana, bands, ColourProfile.Of(ColourBuckets.Green))
        {
        }
    }

    private static RipenessBand Band(int min, int max) =>
        new(min, max, CommercialStatus.ReadyForSale, "Fine.");

    [Fact]
    public void Accepts_bands_that_cover_zero_through_one_hundred()
    {
        var processor = new TestProcessor(Band(0, 50), Band(51, 100));

        Assert.Equal(2, processor.Bands.Count);
    }

    [Fact]
    public void Rejects_a_gap_between_bands()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TestProcessor(Band(0, 50), Band(60, 100)));

        Assert.Contains("gap or overlap", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_overlapping_bands()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TestProcessor(Band(0, 50), Band(40, 100)));

        Assert.Contains("gap or overlap", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_bands_that_do_not_start_at_zero()
    {
        Assert.Throws<ArgumentException>(() => new TestProcessor(Band(1, 100)));
    }

    [Fact]
    public void Rejects_bands_that_stop_short_of_one_hundred()
    {
        var exception = Assert.Throws<ArgumentException>(() => new TestProcessor(Band(0, 99)));

        Assert.Contains("through 100%", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_a_band_that_ends_before_it_starts()
    {
        Assert.Throws<ArgumentException>(
            () => new TestProcessor(Band(0, 50), new RipenessBand(51, 40, CommercialStatus.Expired, "x")));
    }

    [Fact]
    public void Rejects_an_empty_band_list()
    {
        Assert.Throws<ArgumentException>(() => new TestProcessor());
    }

    /// <summary>
    /// The two shipped fruits must satisfy the same rule, so a future edit to
    /// their thresholds cannot leave a score unclassified.
    /// </summary>
    [Fact]
    public void Every_registered_processor_covers_the_whole_scale()
    {
        IFruitProcessor[] processors = { new BananaProcessor(), new AvocadoProcessor() };

        foreach (var processor in processors)
        {
            Assert.Equal(0, processor.Bands[0].MinPercent);
            Assert.Equal(100, processor.Bands[^1].MaxPercent);

            for (var i = 1; i < processor.Bands.Count; i++)
            {
                Assert.Equal(processor.Bands[i - 1].MaxPercent + 1, processor.Bands[i].MinPercent);
            }
        }
    }

    /// <summary>
    /// The guidance is a template. Bands that quote the measured value carry a
    /// {0} placeholder, and every band must survive formatting — a stray brace
    /// in new copy would throw at scan time rather than at review time.
    /// </summary>
    [Fact]
    public void Every_band_guidance_formats_without_throwing()
    {
        IFruitProcessor[] processors = { new BananaProcessor(), new AvocadoProcessor() };

        foreach (var band in processors.SelectMany(processor => processor.Bands))
        {
            var described = band.DescribeFor(band.MinPercent);

            Assert.False(string.IsNullOrWhiteSpace(described));
            Assert.DoesNotContain("{0}", described, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Substitutes_the_measured_percent_into_a_templated_band()
    {
        var band = new RipenessBand(0, 100, CommercialStatus.ActionRequired, "Ripe at {0}%.");

        Assert.Equal("Ripe at 77%.", band.DescribeFor(77));
    }

    [Fact]
    public void Leaves_a_band_without_a_placeholder_untouched()
    {
        var band = new RipenessBand(0, 100, CommercialStatus.ReadyForSale, "Prime shelf.");

        Assert.Equal("Prime shelf.", band.DescribeFor(50));
    }
}
