using System.ComponentModel.DataAnnotations;

namespace Virentum.Api.Contracts.Requests;

/// <summary>
/// Credentials posted to <c>POST /api/auth/login</c>. Validated at the edge;
/// never persisted.
/// </summary>
public sealed record LoginRequest(
    [Required(AllowEmptyStrings = false)]
    string StoreId,
    [Required(AllowEmptyStrings = false)]
    [MinLength(6)]
    string Password);
