using System.Net;

namespace Virentum.Api.Exceptions;

/// <summary>
/// Raised when an inspection request is structurally valid but semantically
/// unacceptable (e.g. an empty image, or an unsupported content type).
/// </summary>
public sealed class InvalidInspectionRequestException : DomainException
{
    public InvalidInspectionRequestException(string message)
        : base(message)
    {
    }

    public override int StatusCode => (int)HttpStatusCode.BadRequest; // 400

    public override string Title => "Invalid inspection request";
}
