using Virentum.Api.Infrastructure.Persistence.Entities;

namespace Virentum.Api.Services.Security;

/// <summary>Issues signed access tokens for authenticated operators.</summary>
public interface ITokenService
{
    /// <summary>Creates a signed JWT for the given account.</summary>
    string CreateAccessToken(UserAccount user);
}
