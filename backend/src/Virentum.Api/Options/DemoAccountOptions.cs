namespace Virentum.Api.Options;

/// <summary>
/// The one account a deployed instance may seed, bound from the "DemoAccount"
/// configuration section.
///
/// A public demo has to be signable-in or it demonstrates nothing, but an API
/// that creates a login on its own is a liability. So both the identifier and
/// the password are required and neither has a default: outside Development the
/// account exists only when someone deliberately set
/// <c>DemoAccount__StoreId</c> and <c>DemoAccount__Password</c> in the
/// environment. Leave them unset and no account is created.
/// </summary>
public sealed class DemoAccountOptions
{
    public const string SectionName = "DemoAccount";

    /// <summary>Login identifier. Null or blank disables seeding entirely.</summary>
    public string? StoreId { get; init; }

    /// <summary>
    /// Plain-text password, hashed before it is stored and never logged. It
    /// belongs in the host's secret store, not in committed configuration.
    /// </summary>
    public string? Password { get; init; }

    public string DisplayName { get; init; } = "Demo Operator";

    public string Station { get; init; } = "Demo station";

    /// <summary>Both halves must be present for the account to be created.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(StoreId) && !string.IsNullOrWhiteSpace(Password);
}
