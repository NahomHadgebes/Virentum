using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Mango policy. A mango is ready when it gives to a gentle squeeze and smells
/// sweet at the stem, and it keeps ripening on the counter after it leaves the
/// shelf — so the ready band is wide and the advice leans on smell and give
/// rather than on the photograph alone.
///
/// One limitation is stated here rather than hidden in the copy: the red blush
/// many varieties carry is sun exposure, not ripeness, and it sits outside the
/// hues the colour stage classifies. On a heavily blushed mango less of the
/// frame lands in any bucket, so the scan reports a thin reading through
/// <see cref="FruitProcessor.AssessEvidence"/> instead of a confident wrong one.
/// Green-to-yellow varieties are read as well as any other fruit here.
/// </summary>
public sealed class MangoProcessor : FruitProcessor
{
    private static readonly RipenessBand[] BandDefinitions =
    {
        new(0, 32, CommercialStatus.Underripe, EdibilityVerdict.NotReadyYet,
            "Hard green",
            "Green over the whole fruit, hard at the shoulders, no scent at the stem.",
            "#4f7a34",
            "Not ready for sale as ready-to-eat. Hold at room temperature and keep it out of the chiller until it softens.",
            "Not ready. Leave it on the counter for 3-6 days and smell the stem end - that tells you more than the colour does."),

        new(33, 64, CommercialStatus.ReadyForSale, EdibilityVerdict.Good,
            "Ready",
            "Colour turned towards gold, gives slightly at the shoulders, faintly sweet at the stem.",
            "#e0a52e",
            "Prime condition for display. A red blush is sun exposure and not a ripeness signal - grade by give, not by colour.",
            "Ready to eat. Cup it in your hand and squeeze gently: it should give like a ripe peach and smell sweet at the stem."),

        new(65, 86, CommercialStatus.ActionRequired, EdibilityVerdict.EatSoon,
            "Full ripe",
            "Deep gold to orange, soft all over, often wrinkled near the stem.",
            "#d97b2b",
            "Ripe at {0}% and soft enough to mark in a stack. Discount now and sell today; wrinkling at the stem is sugar, not spoilage.",
            "At its sweetest right now - eat it today. Wrinkled skin near the stem is a good sign, not a bad one."),

        new(87, 100, CommercialStatus.Expired, EdibilityVerdict.DoNotEat,
            "Overripe",
            "Dark patches spreading over the skin, flesh loose, sap or juice weeping at the stem.",
            "#4a2f1e",
            "Past shelf quality. Remove from display and route to compost / supplier credit log.",
            "Past eating. Large black patches, a sour or alcoholic smell, or flesh that has gone to liquid means throw it out."),
    };

    // A mango moves green to gold to dark, so every bucket the system measures
    // is plausible for it and no colour distribution contradicts the selection.
    // The blush it cannot see is handled as a thin reading, not as a mismatch.
    private static readonly ColourProfile Colours =
        ColourProfile.Of(ColourBuckets.Green, ColourBuckets.Yellow, ColourBuckets.BrownDark);

    private static readonly IReadOnlyDictionary<string, string> Meanings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ColourBuckets.Green] = "unripe - hard at the shoulders",
            [ColourBuckets.Yellow] = "gold - ripe or close to it",
            [ColourBuckets.BrownDark] = "dark patches - past its best",
        };

    public MangoProcessor()
        : base(SupportedFruit.Mango, BandDefinitions, Colours, Meanings)
    {
    }
}
