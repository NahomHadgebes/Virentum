using System.ComponentModel.DataAnnotations;

namespace Virentum.Api.Options;

/// <summary>
/// Strongly-typed JWT settings bound from the "Jwt" configuration section.
/// The <see cref="Secret"/> must come from a secret store / environment
/// variable in any non-development environment — never from committed config.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Audience { get; init; } = string.Empty;

    /// <summary>Symmetric signing key. Minimum 32 chars for HS256.</summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(32, ErrorMessage = "Jwt:Secret must be at least 32 characters for HS256.")]
    public string Secret { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenLifetimeMinutes { get; init; } = 60;
}
