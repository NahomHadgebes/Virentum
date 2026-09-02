namespace Virentum.Api.Domain.Models;

/// <summary>
/// The colour buckets a vision provider may report in
/// <see cref="VisionPrediction.Tags"/>.
///
/// These names were previously written as literals in the colour-analysis
/// service and read by nobody. Naming them here lets a fruit declare which of
/// them it can legitimately present, which is what makes it possible to notice
/// that a picture contradicts the fruit the operator selected.
/// </summary>
public static class ColourBuckets
{
    public const string Green = "green";
    public const string Yellow = "yellow";
    public const string BrownDark = "brownDark";

    /// <summary>Every bucket this system understands.</summary>
    public static IReadOnlySet<string> All { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Green, Yellow, BrownDark };

    /// <summary>The bucket written for a human, for use in operator-facing copy.</summary>
    public static string Describe(string bucket) => bucket.ToLowerInvariant() switch
    {
        "green" => "green",
        "yellow" => "yellow",
        "browndark" => "brown or dark",
        _ => bucket,
    };
}
