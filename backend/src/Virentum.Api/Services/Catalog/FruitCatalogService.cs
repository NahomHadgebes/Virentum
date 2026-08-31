using Virentum.Api.Contracts.Responses;
using Virentum.Api.Domain.Processors;

namespace Virentum.Api.Services.Catalog;

/// <summary>
/// Projects the registered processors into their published profiles.
///
/// The bands come from the processors themselves rather than from a separate
/// table of documentation, so the catalogue describes the thresholds a scan is
/// actually judged against. Adding a fruit changes nothing here.
/// </summary>
public sealed class FruitCatalogService : IFruitCatalogService
{
    private readonly IReadOnlyList<FruitProfileResponse> _profiles;

    public FruitCatalogService(IEnumerable<IFruitProcessor> processors)
    {
        ArgumentNullException.ThrowIfNull(processors);

        _profiles = processors
            .OrderBy(processor => processor.Fruit)
            .Select(processor => new FruitProfileResponse(
                processor.Fruit,
                processor.Bands
                    .Select(band => new RipenessBandResponse(
                        band.MinPercent,
                        band.MaxPercent,
                        band.CommercialStatus,
                        band.Guidance))
                    .ToList()))
            .ToList();
    }

    public IReadOnlyList<FruitProfileResponse> GetProfiles() => _profiles;
}
