namespace Virentum.Api.Domain.Models;

/// <summary>
/// How much weight the reading behind an assessment can carry.
///
/// The colour stage is a heuristic, not a trained model: it counts hues and
/// nothing more. On a good photograph of a fruit's skin that is a fair proxy for
/// ripeness; on a picture that is mostly background, or one dominated by a
/// colour that means nothing for the fruit in question, the number it produces
/// is arithmetic rather than evidence.
///
/// Rather than let a weak reading be presented as a clean verdict, the service
/// reports what it had to work with and why that might not be enough. The
/// assessment is still returned — the operator can see it and judge — but the
/// client is told not to trust it on its own.
/// </summary>
/// <param name="IsReliable">False when at least one concern was raised.</param>
/// <param name="Concerns">
/// Plain statements of what limits this reading, in the order they were found.
/// Empty when nothing limits it.
/// </param>
public sealed record InspectionEvidence(bool IsReliable, IReadOnlyList<string> Concerns)
{
    /// <summary>Nothing stands in the way of taking the reading at face value.</summary>
    public static InspectionEvidence Reliable { get; } = new(true, Array.Empty<string>());

    public static InspectionEvidence From(IReadOnlyList<string> concerns) =>
        concerns.Count == 0 ? Reliable : new InspectionEvidence(false, concerns);
}
