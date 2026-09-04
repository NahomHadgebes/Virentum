using System.Reflection;
using Virentum.Api.Domain.Processors;

namespace Virentum.Api.Tests.Support;

/// <summary>
/// Every fruit processor the API ships, discovered the same way the DI
/// container discovers them.
///
/// Hardcoding the list in each test meant that adding a fruit left assertions
/// like "the catalogue publishes every fruit in the enum" passing against a
/// short list, or failing for a reason that had nothing to do with the new
/// fruit. Reflection keeps those tests honest without anyone remembering to
/// update them.
/// </summary>
public static class RegisteredProcessors
{
    public static IFruitProcessor[] All { get; } = typeof(FruitProcessor).Assembly
        .GetTypes()
        .Where(type => typeof(IFruitProcessor).IsAssignableFrom(type)
                       && type is { IsClass: true, IsAbstract: false })
        .Select(type => (IFruitProcessor)Activator.CreateInstance(type)!)
        .OrderBy(processor => processor.Fruit)
        .ToArray();
}
