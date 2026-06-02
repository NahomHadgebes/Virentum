using System.Net;
using Virentum.Api.Domain.Enums;

namespace Virentum.Api.Exceptions;

/// <summary>
/// Raised when a scan is requested for a fruit that has no registered processor.
/// </summary>
public sealed class UnsupportedFruitException : DomainException
{
    public UnsupportedFruitException(SupportedFruit fruit)
        : base($"No processor is registered for fruit '{fruit}'.")
    {
    }

    public override int StatusCode => (int)HttpStatusCode.UnprocessableEntity; // 422

    public override string Title => "Unsupported fruit";
}
