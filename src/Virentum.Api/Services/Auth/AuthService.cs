using Virentum.Api.Contracts.Requests;
using Virentum.Api.Contracts.Responses;
using Virentum.Api.Exceptions;
using Virentum.Api.Infrastructure.Persistence.Repositories;
using Virentum.Api.Services.Security;

namespace Virentum.Api.Services.Auth;

/// <summary>
/// Default authentication service. Looks the operator up, verifies the password
/// hash in constant time, and maps the entity to a safe <see cref="UserDto"/>
/// (never exposing the password hash).
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByStoreIdAsync(request.StoreId, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Log the attempt without leaking which factor failed.
            _logger.LogWarning("Failed login attempt for store id {StoreId}", request.StoreId);
            throw new AuthenticationFailedException();
        }

        var token = _tokenService.CreateAccessToken(user);
        _logger.LogInformation("Operator {StoreId} authenticated successfully", user.StoreId);

        return new LoginResponse(
            token,
            new UserDto(user.StoreId, user.DisplayName, user.Station));
    }
}
