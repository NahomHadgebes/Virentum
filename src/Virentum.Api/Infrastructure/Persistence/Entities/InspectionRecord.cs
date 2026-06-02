using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Infrastructure.Persistence.Entities;

/// <summary>
/// EF Core entity persisting a single inspection. This is a database concern and
/// must never be returned directly from a controller — it is mapped to an
/// <c>InspectionResponse</c> DTO at the service boundary.
/// </summary>
public class InspectionRecord
{
    public Guid Id { get; set; }

    /// <summary>The authenticated operator / store that performed the scan.</summary>
    public string StoreId { get; set; } = string.Empty;

    public SupportedFruit FruitType { get; set; }

    public int RipenessPercent { get; set; }

    public CommercialStatus CommercialStatus { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public DateTimeOffset ScannedAt { get; set; }
}
