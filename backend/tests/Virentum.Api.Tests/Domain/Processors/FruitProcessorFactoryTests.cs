using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Virentum.Api.Exceptions;
using Virentum.Api.Tests.Support;
using Xunit;

namespace Virentum.Api.Tests.Domain.Processors;

public sealed class FruitProcessorFactoryTests
{
    private static IFruitProcessor[] All => RegisteredProcessors.All;

    [Theory]
    [InlineData(SupportedFruit.Banana)]
    [InlineData(SupportedFruit.Avocado)]
    [InlineData(SupportedFruit.Pear)]
    [InlineData(SupportedFruit.Mango)]
    public void Resolves_the_processor_that_declares_the_requested_fruit(SupportedFruit fruit)
    {
        var processor = new FruitProcessorFactory(All).Create(fruit);

        Assert.Equal(fruit, processor.Fruit);
    }

    /// <summary>
    /// The enum can list a fruit that has no processor. That must surface as a
    /// 422, not as a KeyNotFoundException leaking through as a 500.
    /// </summary>
    [Fact]
    public void Throws_UnsupportedFruit_when_nothing_is_registered_for_the_fruit()
    {
        var factory = new FruitProcessorFactory(new IFruitProcessor[] { new BananaProcessor() });

        var exception = Assert.Throws<UnsupportedFruitException>(
            () => factory.Create(SupportedFruit.Avocado));

        Assert.Equal(422, exception.StatusCode);
        Assert.Contains("Avocado", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Refuses_two_processors_claiming_the_same_fruit()
    {
        var duplicates = new IFruitProcessor[] { new BananaProcessor(), new BananaProcessor() };

        var exception = Assert.Throws<InvalidOperationException>(
            () => new FruitProcessorFactory(duplicates));

        Assert.Contains("exactly one processor", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_a_null_processor_collection()
    {
        Assert.Throws<ArgumentNullException>(() => new FruitProcessorFactory(null!));
    }

    /// <summary>
    /// Every value the API can bind from FruitType must resolve, or a request
    /// the enum accepts would fail at runtime.
    /// </summary>
    [Fact]
    public void Covers_every_value_of_SupportedFruit()
    {
        var factory = new FruitProcessorFactory(All);

        foreach (var fruit in Enum.GetValues<SupportedFruit>())
        {
            var assessment = factory.Create(fruit).Assess(
                new VisionPrediction(fruit, 0.5, new Dictionary<string, double>()),
                Audience.Consumer);

            Assert.False(string.IsNullOrWhiteSpace(assessment.Recommendation));
        }
    }
}
