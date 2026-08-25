namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// The authenticated operator profile returned to the client. Mirrors the
/// frontend <c>VirentumUser</c> shape and deliberately omits any sensitive
/// fields held on the database entity (e.g. password hash).
/// </summary>
public sealed record UserDto(
    string StoreId,
    string DisplayName,
    string Station);
