namespace Virentum.Api.Infrastructure.Persistence.Entities;

/// <summary>
/// EF Core entity for an operator account. The <see cref="PasswordHash"/> never
/// leaves this layer — controllers receive a <c>UserDto</c> instead.
/// </summary>
public class UserAccount
{
    public Guid Id { get; set; }

    /// <summary>Login identifier (store id or email) — unique.</summary>
    public string StoreId { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash of the password. Never serialised.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Station { get; set; } = string.Empty;

    public string Role { get; set; } = "associate";

    public DateTimeOffset CreatedAt { get; set; }
}
