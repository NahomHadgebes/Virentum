using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Pear policy. A pear is picked and sold hard, then ripens off the tree from
/// the inside out — which is why the ready band sits lower than a banana's and
/// the window after it is short. By the time the belly feels soft the flesh
/// around the core is already grainy, so every piece of advice here points at
/// the neck instead.
///
/// The skin runs deep green to yellow to brown, the same axis the colour stage
/// measures, so the reading is as strong for a pear as it is for a banana.
/// </summary>
public sealed class PearProcessor : FruitProcessor
{
    private static readonly RipenessBand[] BandDefinitions =
    {
        new(0, 34, CommercialStatus.Underripe, EdibilityVerdict.NotReadyYet,
            "Hard green",
            "Deep even green, hard everywhere including the neck, stem still tight.",
            "#6f9440",
            "Picked hard and not yet ripe. Hold at room temperature; it will not ripen on a chilled shelf.",
            "Not ready. Leave it out of the fridge for 3-5 days - cold stops a pear ripening."),

        new(35, 66, CommercialStatus.ReadyForSale, EdibilityVerdict.Good,
            "Ready",
            "Green lifting towards yellow, still firm in the belly, the neck just yielding.",
            "#c2cd5c",
            "At its best for display. Rotate to the front and expect a short window - a pear ripens from the core outwards.",
            "Ready. Press the neck by the stem, not the belly: a slight give means eat it today or tomorrow."),

        new(67, 85, CommercialStatus.ActionRequired, EdibilityVerdict.EatSoon,
            "Soft ripe",
            "Yellow, soft at the neck, skin marks under a thumb and bruises show as brown patches.",
            "#dfae42",
            "Ripe at {0}% and bruising easily. Discount now and move it off the stack so the weight stops marking the fruit underneath.",
            "Very juicy and sweet - eat it today. Bruised patches can be cut away; the rest is fine."),

        new(86, 100, CommercialStatus.Expired, EdibilityVerdict.DoNotEat,
            "Breaking down",
            "Brown and sunken, the flesh mealy or fermenting, often leaking at the base.",
            "#6b4b2a",
            "Past shelf quality. Remove from display and route to compost / supplier credit log.",
            "Past eating. Grainy brown flesh or a fermented smell means throw it out."),
    };

    // A pear travels green to yellow to brown, so it can legitimately present
    // every bucket this system measures. Like a banana, it can therefore never
    // raise a colour mismatch - which is the honest outcome, not a gap.
    private static readonly ColourProfile Colours =
        ColourProfile.Of(ColourBuckets.Green, ColourBuckets.Yellow, ColourBuckets.BrownDark);

    private static readonly IReadOnlyDictionary<string, string> Meanings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ColourBuckets.Green] = "still hard - ripening has not started",
            [ColourBuckets.Yellow] = "ripe - the neck should be giving by now",
            [ColourBuckets.BrownDark] = "bruised or breaking down",
        };

    public PearProcessor()
        : base(SupportedFruit.Pear, BandDefinitions, Colours, Meanings)
    {
    }
}
