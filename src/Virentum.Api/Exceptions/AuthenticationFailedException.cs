using System.Net;

namespace Virentum.Api.Exceptions;

/// <summary>
/// Raised when supplied credentials are invalid. The message is intentionally
/// generic so it never reveals whether the store id or the password was wrong.
/// </summary>
public sealed class AuthenticationFailedException : DomainException
{
    public AuthenticationFailedException()
        : base("Invalid store id or password.")
    {
    }

    public override int StatusCode => (int)HttpStatusCode.Unauthorized; // 401

    public override string Title => "Authentication failed";
}
