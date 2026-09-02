using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Avocado policy. Avocados darken from green towards black as they ripen, are
/// sold firm, and rely on the cold chain — so the advice references temperature
/// and the "ready" band runs higher than a banana's.
///
/// Colour is a weaker signal here than for a banana: a dark avocado can be
/// perfectly ripe or spoiled, and only pressing it tells you which. The consumer
/// guidance says so rather than pretending the photograph settled it.
/// </summary>
public sealed class AvocadoProcessor : FruitProcessor
{
    private static readonly RipenessBand[] BandDefinitions =
    {
        new(0, 35, CommercialStatus.Underripe, EdibilityVerdict.NotReadyYet,
            "Bright green",
            "Vivid green skin, rock hard, no give anywhere under firm thumb pressure.",
            "#5c9c3f",
            "Still firm and bright green - not yet ripe. Hold for ripening; not ready for the shelf.",
            "Not ready. Keep it on the counter for 2-4 days; a paper bag with a banana speeds it up."),

        new(36, 82, CommercialStatus.ReadyForSale, EdibilityVerdict.Good,
            "Ready",
            "Darker green, yields slightly to gentle pressure in your palm - not your fingertips.",
            "#4b6b35",
            "Firm and ready for the premium produce section. Maintain at 4 degrees C.",
            "Good to eat. Press gently with your whole hand: it should give a little, not dent."),

        new(83, 93, CommercialStatus.ActionRequired, EdibilityVerdict.EatSoon,
            "Soft",
            "Dark, noticeably soft, skin may dimple where it was handled.",
            "#3b3326",
            "Ripening fast at {0}%. Apply a 50% discount label and move to the 'ready to eat' basket for same-day sale.",
            "Use it today. Fine for guacamole even if a little is browned - cut that part away."),

        new(94, 100, CommercialStatus.Expired, EdibilityVerdict.DoNotEat,
            "Overripe",
            "Very dark and mushy, dents easily, flesh brown or stringy throughout.",
            "#241f18",
            "Overripe and bruising. Remove from display and route to compost / supplier credit log.",
            "Cut it open before deciding: brown streaks throughout, a sour smell or mould means throw it out."),
    };

    // An avocado darkens green to near-black and never turns yellow, so a
    // yellow-dominated image is evidence the operator selected the wrong fruit,
    // or photographed cut flesh rather than skin.
    private static readonly ColourProfile Colours =
        ColourProfile.Of(ColourBuckets.Green, ColourBuckets.BrownDark);

    private static readonly IReadOnlyDictionary<string, string> Meanings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ColourBuckets.Green] = "firm and under-ripe",
            [ColourBuckets.BrownDark] = "ripe to over-ripe - colour alone cannot separate the two",
            [ColourBuckets.Yellow] = "not a stage an avocado skin passes through",
        };

    public AvocadoProcessor()
        : base(SupportedFruit.Avocado, BandDefinitions, Colours, Meanings)
    {
    }
}
