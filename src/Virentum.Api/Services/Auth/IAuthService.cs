using Virentum.Api.Contracts.Requests;
using Virentum.Api.Contracts.Responses;

namespace Virentum.Api.Services.Auth;

/// <summary>Authenticates operators and issues access tokens.</summary>
public interface IAuthService
{
    /// <summary>
    /// Validates credentials and returns a token + profile.
    /// </summary>
    /// <exception cref="Exceptions.AuthenticationFailedException">
    /// Thrown when the credentials are invalid.
    /// </exception>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
