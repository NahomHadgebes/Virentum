using Virentum.Api.Options;
using Xunit;

namespace Virentum.Api.Tests.Options;

/// <summary>
/// IsConfigured is the gate that decides whether a deployed instance has an
/// account at all. Half-configured must count as not configured: seeding a
/// login with a blank password because someone set only the store id is the
/// failure this guards against.
/// </summary>
public sealed class DemoAccountOptionsTests
{
    [Fact]
    public void Is_not_configured_when_nothing_is_set()
    {
        Assert.False(new DemoAccountOptions().IsConfigured);
    }

    [Theory]
    [InlineData("demo-store", null)]
    [InlineData(null, "a-password")]
    [InlineData("demo-store", "")]
    [InlineData("", "a-password")]
    [InlineData("demo-store", "   ")]
    [InlineData("   ", "a-password")]
    public void Is_not_configured_when_only_one_half_is_supplied(string? storeId, string? password)
    {
        var options = new DemoAccountOptions { StoreId = storeId, Password = password };

        Assert.False(options.IsConfigured);
    }

    [Fact]
    public void Is_configured_when_both_halves_are_supplied()
    {
        var options = new DemoAccountOptions { StoreId = "demo-store", Password = "a-password" };

        Assert.True(options.IsConfigured);
    }

    [Fact]
    public void Carries_a_usable_display_name_and_station_without_being_told()
    {
        var options = new DemoAccountOptions { StoreId = "demo-store", Password = "a-password" };

        Assert.False(string.IsNullOrWhiteSpace(options.DisplayName));
        Assert.False(string.IsNullOrWhiteSpace(options.Station));
    }
}
