using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// One row of a store's scan history. Carries the same assessment fields as
/// <see cref="InspectionResponse"/> plus the record id, which the client needs
/// as a stable key when rendering a list.
/// </summary>
public sealed record InspectionHistoryItem(
    Guid Id,
    SupportedFruit FruitType,
    int RipenessPercent,
    CommercialStatus CommercialStatus,
    string Recommendation,
    DateTimeOffset ScannedAt);
