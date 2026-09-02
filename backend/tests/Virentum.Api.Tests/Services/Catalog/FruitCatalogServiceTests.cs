using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Processors;
using Virentum.Api.Services.Catalog;
using Xunit;

namespace Virentum.Api.Tests.Services.Catalog;

public sealed class FruitCatalogServiceTests
{
    private static FruitCatalogService Catalog() =>
        new(new IFruitProcessor[] { new AvocadoProcessor(), new BananaProcessor() });

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
            Assert.Equal(banana.Bands[i].Guidance, profile.Bands[i].GuidanceTemplate);
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

    [Fact]
    public void Rejects_a_null_processor_collection()
    {
        Assert.Throws<ArgumentNullException>(() => new FruitCatalogService(null!));
    }
}
