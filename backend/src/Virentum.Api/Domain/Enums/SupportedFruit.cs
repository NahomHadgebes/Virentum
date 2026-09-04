namespace Virentum.Api.Domain.Enums;

/// <summary>
/// The catalogue of produce the platform can currently inspect.
///
/// Adding a new value here is intentionally NOT enough to make the system
/// process that fruit — a dedicated <c>IFruitProcessor</c> implementation must
/// also be supplied. This keeps the commercial logic for every fruit isolated
/// in its own class (Open/Closed Principle) rather than leaking into switches.
/// </summary>
public enum SupportedFruit
{
    Banana,
    Avocado,
    Pear,
    Mango,
}
