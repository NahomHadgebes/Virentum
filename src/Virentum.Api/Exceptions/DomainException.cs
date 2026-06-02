namespace Virentum.Api.Exceptions;

/// <summary>
/// Base type for all expected, business-rule violations. The global exception
/// handler maps these to clean HTTP Problem Details responses; anything that is
/// NOT a <see cref="DomainException"/> is treated as an unexpected server fault
/// and is never surfaced to the client in detail.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>The HTTP status code this failure should map to.</summary>
    public abstract int StatusCode { get; }

    /// <summary>A short, client-safe title for the Problem Details payload.</summary>
    public abstract string Title { get; }

    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
