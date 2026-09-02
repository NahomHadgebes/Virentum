namespace Virentum.Api.Domain.Models;

/// <summary>
/// The colours a fruit can legitimately present across its whole shelf life.
///
/// Virentum does not identify produce — the operator selects the fruit and the
/// vision stage only measures colour. This profile is what lets the system
/// notice the clearest case of those two disagreeing: an image dominated by a
/// colour the declared fruit never takes.
///
/// It is deliberately weak. A green banana and an unripe avocado are both
/// green-dominated and cannot be told apart this way, so a fruit whose profile
/// covers every bucket can never raise a mismatch. That is honest: the check
/// catches contradictions, not identity.
/// </summary>
/// <param name="PlausibleBuckets">
/// Buckets from <see cref="ColourBuckets"/> this fruit can show. Anything
/// outside the list counts towards a mismatch.
/// </param>
public sealed record ColourProfile(IReadOnlySet<string> PlausibleBuckets)
{
    public static ColourProfile Of(params string[] buckets) =>
        new(new HashSet<string>(buckets, StringComparer.OrdinalIgnoreCase));
}
