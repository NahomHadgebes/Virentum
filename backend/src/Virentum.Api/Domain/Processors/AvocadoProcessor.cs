using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Domain.Processors;

/// <summary>
/// Avocado-specific merchandising policy. Avocados darken green to dark to black
/// as they ripen, are sold firm and rely on the cold chain, so the advice
/// references temperature and the "ready" band runs higher than for bananas.
///
/// These bands are the only place the avocado thresholds exist.
/// </summary>
public sealed class AvocadoProcessor : FruitProcessor
{
    private static readonly RipenessBand[] BandDefinitions =
    {
        new(0, 35, CommercialStatus.Underripe,
            "Still firm and bright green — not yet ripe. Hold for ripening; " +
            "not ready for the shelf."),

        new(36, 82, CommercialStatus.ReadyForSale,
            "Firm and ready for the premium produce section. Maintain at 4°C."),

        new(83, 93, CommercialStatus.ActionRequired,
            "Ripening fast at {0}%. Apply a 50% discount label and move to the " +
            "'ready to eat' basket for same-day sale."),

        new(94, 100, CommercialStatus.Expired,
            "Overripe and bruising. Remove from display and route to compost / " +
            "supplier credit log."),
    };

    public AvocadoProcessor()
        : base(SupportedFruit.Avocado, BandDefinitions)
    {
    }
}
