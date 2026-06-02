namespace Virentum.Api.Contracts.Responses;

/// <summary>
/// Successful authentication payload: a signed access token plus the operator
/// profile the frontend caches for the session.
/// </summary>
public sealed record LoginResponse(
    string Token,
    UserDto User);
