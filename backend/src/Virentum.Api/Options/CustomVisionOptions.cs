using System.ComponentModel.DataAnnotations;

namespace Virentum.Api.Options;

/// <summary>
/// Strongly-typed Azure Custom Vision settings bound from the "CustomVision"
/// configuration section. Keys and endpoints are secrets and must be supplied
/// via environment variables / a secret store at runtime.
///
/// The credential fields are validated only when the real service is in use
/// (<see cref="UseStub"/> == false), so local/dev runs work without provisioned
/// cloud credentials.
/// </summary>
public sealed class CustomVisionOptions : IValidatableObject
{
    public const string SectionName = "CustomVision";

    /// <summary>Prediction endpoint base URL, e.g. https://&lt;region&gt;.cognitiveservices.azure.com.</summary>
    public string Endpoint { get; init; } = string.Empty;

    public string PredictionKey { get; init; } = string.Empty;

    public string ProjectId { get; init; } = string.Empty;

    public string PublishedModelName { get; init; } = string.Empty;

    /// <summary>
    /// When true (the default for local/dev), the vision service returns a
    /// deterministic stub instead of calling Azure, so the API is usable
    /// without provisioned cloud credentials.
    /// </summary>
    public bool UseStub { get; init; } = true;

    [Range(1, 120)]
    public int TimeoutSeconds { get; init; } = 30;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UseStub)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(Endpoint) || !Uri.IsWellFormedUriString(Endpoint, UriKind.Absolute))
        {
            yield return new ValidationResult(
                "CustomVision:Endpoint must be a valid absolute URL when UseStub is false.",
                new[] { nameof(Endpoint) });
        }

        if (string.IsNullOrWhiteSpace(PredictionKey))
        {
            yield return new ValidationResult(
                "CustomVision:PredictionKey is required when UseStub is false.",
                new[] { nameof(PredictionKey) });
        }

        if (string.IsNullOrWhiteSpace(ProjectId))
        {
            yield return new ValidationResult(
                "CustomVision:ProjectId is required when UseStub is false.",
                new[] { nameof(ProjectId) });
        }

        if (string.IsNullOrWhiteSpace(PublishedModelName))
        {
            yield return new ValidationResult(
                "CustomVision:PublishedModelName is required when UseStub is false.",
                new[] { nameof(PublishedModelName) });
        }
    }
}
