namespace Virentum.Api.Domain.Models;

/// <summary>
/// One measured contribution to an assessment, in the terms the reader can check
/// against the photograph they just took.
///
/// This is what turns a number into an explanation: not "68% ripe" alone, but
/// "just over half of what we could see was yellow". A reader who disagrees can
/// look at their own picture and say so.
/// </summary>
/// <param name="Label">The colour bucket, written for a human.</param>
/// <param name="Share">Its share of everything the analysis could classify, 0-1.</param>
/// <param name="Meaning">What that colour indicates for this particular fruit.</param>
public sealed record AnalysisFactor(string Label, double Share, string Meaning);
