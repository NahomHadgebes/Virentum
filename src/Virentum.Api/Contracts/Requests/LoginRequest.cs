using System.ComponentModel.DataAnnotations;

namespace Virentum.Api.Contracts.Requests;

/// <summary>
/// Credentials posted to <c>POST /api/auth/login</c>. Validated at the edge;
/// never persisted.
/// </summary>
public sealed record LoginRequest(
    [property: Required(AllowEmptyStrings = false)]
    string StoreId,
    [property: Required(AllowEmptyStrings = false)]
    [property: MinLength(6)]
    string Password);
