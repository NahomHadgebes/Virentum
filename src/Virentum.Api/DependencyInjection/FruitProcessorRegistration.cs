using System.Reflection;
using Virentum.Api.Domain.Processors;

namespace Virentum.Api.DependencyInjection;

/// <summary>
/// Registers the fruit-processor strategy set. Implementations are discovered by
/// scanning the assembly for <see cref="IFruitProcessor"/> types, so introducing
/// a new fruit requires only adding a new processor class — this method, the
/// factory and the controllers stay untouched (Open/Closed Principle).
/// </summary>
public static class FruitProcessorRegistration
{
    public static IServiceCollection AddFruitProcessors(this IServiceCollection services)
    {
        var processorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IFruitProcessor).IsAssignableFrom(t)
                        && t is { IsClass: true, IsAbstract: false });

        foreach (var type in processorTypes)
        {
            // Processors are stateless ⇒ safe and efficient as singletons.
            services.AddSingleton(typeof(IFruitProcessor), type);
        }

        services.AddSingleton<IFruitProcessorFactory, FruitProcessorFactory>();
        return services;
    }
}
