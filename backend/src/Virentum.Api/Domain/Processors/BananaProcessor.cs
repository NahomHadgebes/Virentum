using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Banana-specific merchandising policy. Bananas progress green to yellow to
/// spotted to black, so the sweet spot is the middle: too green is unripe, too
/// dark is spoiled, and because they brown quickly once past peak the
/// "action required" window is wide and the advice is display-oriented.
///
/// These bands are the only place the banana thresholds exist.
/// </summary>
public sealed class BananaProcessor : FruitProcessor
{
    private static readonly RipenessBand[] BandDefinitions =
    {
        new(0, 42, CommercialStatus.Underripe,
            "Still green and underripe. Hold off the shelf and allow 1–3 days " +
            "to ripen before display."),

        new(43, 75, CommercialStatus.ReadyForSale,
            "Perfect condition. Suitable for prime display at the front shelf."),

        new(76, 88, CommercialStatus.ActionRequired,
            "This batch is {0}% ripe. Print a 50% discount label immediately to " +
            "ensure sale within 24 hours."),

        new(89, 100, CommercialStatus.Expired,
            "Quality below shelf threshold. Remove from display and route to " +
            "compost / supplier credit log."),
    };

    // A banana passes through every bucket this system measures on its way from
    // green to black, so no colour distribution contradicts a banana. This
    // profile can therefore never raise a mismatch — which is the honest
    // outcome, not an omission.
    private static readonly ColourProfile Colours =
        ColourProfile.Of(ColourBuckets.Green, ColourBuckets.Yellow, ColourBuckets.BrownDark);

    public BananaProcessor()
        : base(SupportedFruit.Banana, BandDefinitions, Colours)
    {
    }
}
