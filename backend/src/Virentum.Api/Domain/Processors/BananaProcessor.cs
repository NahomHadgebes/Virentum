using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Banana policy. Bananas run green to yellow to spotted to black, so the sweet
/// spot is the middle, and because they brown quickly once past peak the
/// discount window is wide. These bands are the only place the thresholds exist.
///
/// A banana a shop must pull is often still perfectly good for baking, which is
/// why the commercial and edible readings diverge at the top of the scale.
/// </summary>
public sealed class BananaProcessor : FruitProcessor
{
    private static readonly RipenessBand[] BandDefinitions =
    {
        new(0, 42, CommercialStatus.Underripe, EdibilityVerdict.NotReadyYet,
            "Green",
            "Firm, with green skin and no give when pressed. The stem end is still stiff.",
            "#7cb342",
            "Still green and underripe. Hold off the shelf and allow 1-3 days to ripen before display.",
            "Not ready yet. Leave it out at room temperature for a couple of days - it will get sweeter."),

        new(43, 75, CommercialStatus.ReadyForSale, EdibilityVerdict.Good,
            "Prime",
            "Even yellow, firm but with a slight give. This is a banana at its best.",
            "#ffd54f",
            "Perfect condition. Suitable for prime display at the front shelf.",
            "Ready to eat right now. Sweet, firm and at its best."),

        new(76, 88, CommercialStatus.ActionRequired, EdibilityVerdict.EatSoon,
            "Spotted",
            "Yellow with brown freckles, noticeably softer. Sweeter than it looks.",
            "#c98a3a",
            "This batch is {0}% ripe. Print a 50% discount label immediately to ensure sale within 24 hours.",
            "Very sweet and soft. Eat it today, or peel and freeze it for baking and smoothies."),

        new(89, 100, CommercialStatus.Expired, EdibilityVerdict.DoNotEat,
            "Blackened",
            "Skin largely black, flesh mushy, often with a fermented smell.",
            "#4e342e",
            "Quality below shelf threshold. Remove from display and route to compost / supplier credit log.",
            "Past eating. If the flesh smells fermented or is leaking, throw it out."),
    };

    // A banana passes through every bucket this system measures on its way from
    // green to black, so no colour distribution contradicts a banana. This
    // profile can therefore never raise a mismatch - the honest outcome, not an
    // omission.
    private static readonly ColourProfile Colours =
        ColourProfile.Of(ColourBuckets.Green, ColourBuckets.Yellow, ColourBuckets.BrownDark);

    private static readonly IReadOnlyDictionary<string, string> Meanings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ColourBuckets.Green] = "unripe - still starchy",
            [ColourBuckets.Yellow] = "ripe - the sweet spot",
            [ColourBuckets.BrownDark] = "over-ripe - sugars breaking down",
        };

    public BananaProcessor()
        : base(SupportedFruit.Banana, BandDefinitions, Colours, Meanings)
    {
    }
}
