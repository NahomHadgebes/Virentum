using Virentum.Api.Contracts.Responses;

namespace Virentum.Api.Services.Catalog;

/// <summary>
/// Publishes the ripeness policy of every supported fruit.
/// </summary>
public interface IFruitCatalogService
{
    /// <summary>Every registered fruit, in enum declaration order.</summary>
    IReadOnlyList<FruitProfileResponse> GetProfiles();
}
