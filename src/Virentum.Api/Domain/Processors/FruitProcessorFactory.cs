using Virentum.Api.Domain.Enums;
using Virentum.Api.Exceptions;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Dictionary-backed factory. It receives every registered
/// <see cref="IFruitProcessor"/> from the DI container and indexes them by the
/// fruit each one declares it handles.
///
/// Because discovery is driven entirely by what is registered in the container
/// (see <c>ServiceCollectionExtensions.AddFruitProcessors</c>), adding a new
/// fruit never requires editing this class — there are deliberately no
/// switch-statements or hardcoded fruit names here.
/// </summary>
public sealed class FruitProcessorFactory : IFruitProcessorFactory
{
    private readonly IReadOnlyDictionary<SupportedFruit, IFruitProcessor> _processors;

    public FruitProcessorFactory(IEnumerable<IFruitProcessor> processors)
    {
        ArgumentNullException.ThrowIfNull(processors);

        var map = new Dictionary<SupportedFruit, IFruitProcessor>();
        foreach (var processor in processors)
        {
            if (!map.TryAdd(processor.Fruit, processor))
            {
                throw new InvalidOperationException(
                    $"More than one IFruitProcessor is registered for '{processor.Fruit}'. " +
                    "Each fruit must have exactly one processor.");
            }
        }

        _processors = map;
    }

    public IFruitProcessor Create(SupportedFruit fruit) =>
        _processors.TryGetValue(fruit, out var processor)
            ? processor
            : throw new UnsupportedFruitException(fruit);
}
