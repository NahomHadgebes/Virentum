using System.Globalization;
using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Models;

/// <summary>
/// One ripeness interval, everything the system knows about it, and the two ways
/// of saying it.
///
/// Thresholds used to live as private constants inside each processor, reachable
/// only by running an inspection. Expressing the whole stage as data lets the API
/// publish a fruit's full policy while the processor still evaluates from the
/// same list — the guide and the verdict cannot drift apart.
/// </summary>
/// <param name="MinPercent">First ripeness percent in the band, inclusive.</param>
/// <param name="MaxPercent">Last ripeness percent in the band, inclusive.</param>
/// <param name="CommercialStatus">The merchandising decision for a store.</param>
/// <param name="Edibility">The answer for someone about to eat it.</param>
/// <param name="StageName">What this stage is called for this fruit.</param>
/// <param name="Appearance">How the fruit looks and feels here, so a reader can check.</param>
/// <param name="SwatchHex">Representative colour of the fruit at this stage.</param>
/// <param name="BusinessGuidance">
/// Advice for a store. A <c>{0}</c> placeholder, where present, is replaced by
/// the measured percent.
/// </param>
/// <param name="ConsumerGuidance">Advice for a person, in plain language.</param>
public sealed record RipenessBand(
    int MinPercent,
    int MaxPercent,
    CommercialStatus CommercialStatus,
    EdibilityVerdict Edibility,
    string StageName,
    string Appearance,
    string SwatchHex,
    string BusinessGuidance,
    string ConsumerGuidance)
{
    /// <summary>The advice with the measured value substituted, if it asks for one.</summary>
    public string DescribeFor(int ripenessPercent, Audience audience)
    {
        var template = audience == Audience.Consumer ? ConsumerGuidance : BusinessGuidance;
        return string.Format(CultureInfo.InvariantCulture, template, ripenessPercent);
    }
}
