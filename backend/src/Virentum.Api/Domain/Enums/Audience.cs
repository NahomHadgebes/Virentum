namespace Virentum.Api.Domain.Enums;

/// <summary>
/// Who the assessment is being written for. The measurement is identical either
/// way — the same pixels, the same bands — but the question differs. A shopper
/// asks whether the fruit is still good; a store asks what to do with the stock.
///
/// The string names are part of the public API contract.
/// </summary>
public enum Audience
{
    /// <summary>A person holding one piece of fruit.</summary>
    Consumer,

    /// <summary>A store, restaurant or distributor holding many.</summary>
    Business,
}
