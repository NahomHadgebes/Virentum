using Microsoft.AspNetCore.Http;
using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Contracts.Requests;

/// <summary>
/// Inbound multipart/form-data payload for an inspection scan. This DTO is the
/// API's edge contract — it is bound from the HTTP form and never reaches the
/// database. The field names match exactly what the frontend submits
/// (<c>Images</c>, <c>FruitType</c>, <c>Audience</c>).
/// </summary>
public sealed class ScanRequest
{
    /// <summary>
    /// One to three photographs of the same item. Evidence is pooled across all
    /// of them, so a second angle or a picture of the inside strengthens the
    /// reading rather than replacing it.
    /// </summary>
    public IFormFileCollection? Images { get; init; }

    /// <summary>Which fruit the operator selected. Bound from its string name.</summary>
    public SupportedFruit FruitType { get; init; }

    /// <summary>
    /// Who the answer is for. The measurement is the same either way; the
    /// wording, the headline and the level of detail are not.
    /// </summary>
    public Audience Audience { get; init; } = Audience.Consumer;
}
