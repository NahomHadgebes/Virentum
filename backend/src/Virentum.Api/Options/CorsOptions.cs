namespace Virentum.Api.Options;

/// <summary>
/// Strongly-typed CORS settings bound from the "Cors" configuration section.
/// The allowed origins list is environment-specific: localhost in development,
/// the real frontend domain in production.
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>The exact set of origins permitted to call the API.</summary>
    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();
}
