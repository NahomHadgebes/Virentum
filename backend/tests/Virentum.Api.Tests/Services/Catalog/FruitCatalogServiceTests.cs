using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Processors;
using Virentum.Api.Services.Catalog;
using Virentum.Api.Tests.Support;
using Xunit;

namespace Virentum.Api.Tests.Services.Catalog;

public sealed class FruitCatalogServiceTests
{
    // Deliberately reversed: the catalogue must order by the enum, not by the
    // order the container happened to hand the processors over in.
    private static FruitCatalogService Catalog() =>
        new(RegisteredProcessors.All.AsEnumerable().Reverse());

    [Fact]
    public void Publishes_every_registered_fruit()
    {
        var profiles = Catalog().GetProfiles();

        Assert.Equal(Enum.GetValues<SupportedFruit>().Length, profiles.Count);
    }

    [Fact]
    public void Orders_fruits_by_the_enum_regardless_of_registration_order()
    {
        var profiles = Catalog().GetProfiles();

        Assert.Equal(SupportedFruit.Banana, profiles[0].FruitType);
        Assert.Equal(SupportedFruit.Avocado, profiles[1].FruitType);
    }

    /// <summary>
    /// The whole reason the bands became data: what the catalogue publishes must
    /// be the very thresholds a scan is judged against, not a second copy that
    /// can drift.
    /// </summary>
    [Fact]
    public void Publishes_the_same_thresholds_the_processor_evaluates()
    {
        var banana = new BananaProcessor();
        var profile = Catalog().GetProfiles().Single(entry => entry.FruitType == SupportedFruit.Banana);

        Assert.Equal(banana.Bands.Count, profile.Bands.Count);

        for (var i = 0; i < banana.Bands.Count; i++)
        {
            Assert.Equal(banana.Bands[i].MinPercent, profile.Bands[i].MinPercent);
            Assert.Equal(banana.Bands[i].MaxPercent, profile.Bands[i].MaxPercent);
            Assert.Equal(banana.Bands[i].CommercialStatus, profile.Bands[i].CommercialStatus);
            Assert.Equal(banana.Bands[i].BusinessGuidance, profile.Bands[i].BusinessGuidance);
            Assert.Equal(banana.Bands[i].ConsumerGuidance, profile.Bands[i].ConsumerGuidance);
            Assert.Equal(banana.Bands[i].StageName, profile.Bands[i].StageName);
            Assert.Equal(banana.Bands[i].Edibility, profile.Bands[i].Edibility);
        }
    }

    [Fact]
    public void Publishes_bands_covering_the_whole_scale_for_every_fruit()
    {
        foreach (var profile in Catalog().GetProfiles())
        {
            Assert.Equal(0, profile.Bands[0].MinPercent);
            Assert.Equal(100, profile.Bands[^1].MaxPercent);
        }
    }

    /// <summary>
    /// The guide is only useful if it can show what a stage looks like, so every
    /// stage has to carry the pieces a reader checks against real fruit.
    /// </summary>
    [Fact]
    public void Every_stage_carries_what_a_guide_needs_to_show_it()
    {
        foreach (var band in Catalog().GetProfiles().SelectMany(profile => profile.Bands))
        {
            Assert.False(string.IsNullOrWhiteSpace(band.StageName));
            Assert.False(string.IsNullOrWhiteSpace(band.Appearance));
            Assert.False(string.IsNullOrWhiteSpace(band.BusinessGuidance));
            Assert.False(string.IsNullOrWhiteSpace(band.ConsumerGuidance));
            Assert.Matches("^#[0-9a-fA-F]{6}$", band.SwatchHex);
        }
    }

    /// <summary>
    /// A shop and a shopper are answering different questions, so the two
    /// sentences must not be the same sentence.
    /// </summary>
    [Fact]
    public void Words_every_stage_differently_for_the_two_audiences()
    {
        foreach (var band in Catalog().GetProfiles().SelectMany(profile => profile.Bands))
        {
            Assert.NotEqual(band.BusinessGuidance, band.ConsumerGuidance);
        }
    }

    [Fact]
    public void Rejects_a_null_processor_collection()
    {
        Assert.Throws<ArgumentNullException>(() => new FruitCatalogService(null!));
    }
}
