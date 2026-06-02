namespace Virentum.Api.Domain.Enums;

/// <summary>
/// The merchandising decision derived from a ripeness assessment. The string
/// names of these members form part of the public API contract consumed by the
/// Virentum frontend, so they must remain stable.
/// </summary>
public enum CommercialStatus
{
    /// <summary>Fit for full-price display on the shelf.</summary>
    ReadyForSale,

    /// <summary>Sellable but degrading — discount / reroute promptly.</summary>
    ActionRequired,

    /// <summary>Below shelf threshold — pull from display.</summary>
    Expired,
}
