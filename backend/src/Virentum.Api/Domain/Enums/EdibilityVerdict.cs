namespace Virentum.Api.Domain.Enums;

/// <summary>
/// The consumer-facing answer to "can I still eat this?".
///
/// Deliberately not the same scale as <see cref="CommercialStatus"/>: produce a
/// shop must pull from display is often perfectly good at home, and conflating
/// the two would either waste food or mislead a shopper. The string names are
/// part of the public API contract.
/// </summary>
public enum EdibilityVerdict
{
    /// <summary>Edible, but it will taste better in a few days.</summary>
    NotReadyYet,

    /// <summary>At its best right now.</summary>
    Good,

    /// <summary>Still fine, but use it today or tomorrow.</summary>
    EatSoon,

    /// <summary>Past the point where it should be eaten.</summary>
    DoNotEat,
}
