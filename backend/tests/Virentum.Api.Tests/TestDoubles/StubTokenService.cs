using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Services.Security;

namespace Virentum.Api.Tests.TestDoubles;

/// <summary>
/// Issues a predictable token. JwtTokenService needs real signing configuration
/// and is not what AuthService's tests are about.
/// </summary>
internal sealed class StubTokenService : ITokenService
{
    public const string Token = "stub.access.token";

    public UserAccount? IssuedFor { get; private set; }

    public string CreateAccessToken(UserAccount user)
    {
        IssuedFor = user;
        return Token;
    }
}
