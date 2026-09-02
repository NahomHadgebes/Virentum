using System.Globalization;
using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Domain.Models;

/// <summary>
/// One ripeness interval and the merchandising decision it carries.
///
/// Thresholds used to live as private constants inside each processor, which
/// meant they could only be reached by running an inspection. Expressing them as
/// data lets the API publish a fruit's full policy while the processor still
/// evaluates from that same list — the two cannot drift apart.
/// </summary>
/// <param name="MinPercent">First ripeness percent in the band, inclusive.</param>
/// <param name="MaxPercent">Last ripeness percent in the band, inclusive.</param>
/// <param name="CommercialStatus">The decision for produce in this band.</param>
/// <param name="Guidance">
/// Advice for the store associate. A <c>{0}</c> placeholder, where present, is
/// replaced by the measured percent.
/// </param>
public sealed record RipenessBand(
    int MinPercent,
    int MaxPercent,
    CommercialStatus CommercialStatus,
    string Guidance)
{
    /// <summary>The advice with the measured value substituted, if it asks for one.</summary>
    public string DescribeFor(int ripenessPercent) =>
        string.Format(CultureInfo.InvariantCulture, Guidance, ripenessPercent);
}
