using Microsoft.AspNetCore.Http;
using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Requests;

/// <summary>
/// Inbound multipart/form-data payload for an inspection scan. This DTO is the
/// API's edge contract — it is bound from the HTTP form and never reaches the
/// database. The field names match exactly what the frontend submits
/// (<c>Image</c>, <c>FruitType</c>).
/// </summary>
public sealed class ScanRequest
{
    /// <summary>The captured produce photo to analyse.</summary>
    public IFormFile? Image { get; init; }

    /// <summary>Which fruit the operator selected. Bound from its string name.</summary>
    public SupportedFruit FruitType { get; init; }
}
