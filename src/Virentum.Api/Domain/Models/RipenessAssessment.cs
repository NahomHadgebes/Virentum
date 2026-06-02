using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Models;

/// <summary>
/// The commercial verdict produced by an <see cref="Processors.IFruitProcessor"/>.
/// Immutable by design so it can be passed across layers without defensive copies.
/// </summary>
/// <param name="RipenessPercent">Ripeness expressed as a whole percentage [0, 100].</param>
/// <param name="CommercialStatus">The merchandising decision.</param>
/// <param name="Recommendation">Human-readable action for the store associate.</param>
public sealed record RipenessAssessment(
    int RipenessPercent,
    CommercialStatus CommercialStatus,
    string Recommendation);
