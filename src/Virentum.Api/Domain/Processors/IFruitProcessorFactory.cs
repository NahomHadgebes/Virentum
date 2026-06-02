using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Resolves the correct <see cref="IFruitProcessor"/> for a requested fruit.
/// Consumers depend on this abstraction rather than on concrete processors,
/// so the set of supported fruits can grow without changing call sites.
/// </summary>
public interface IFruitProcessorFactory
{
    /// <summary>
    /// Returns the processor registered for <paramref name="fruit"/>.
    /// </summary>
    /// <exception cref="Exceptions.UnsupportedFruitException">
    /// Thrown when no processor is registered for the requested fruit.
    /// </exception>
    IFruitProcessor Create(SupportedFruit fruit);
}
