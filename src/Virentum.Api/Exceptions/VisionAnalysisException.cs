using System.Net;

namespace Virentum.Api.Exceptions;

/// <summary>
/// Raised when the upstream computer-vision provider cannot be reached or
/// returns an unusable response. Mapped to 502 Bad Gateway because the fault
/// originates in a dependency, not in the caller's request.
/// </summary>
public sealed class VisionAnalysisException : DomainException
{
    public VisionAnalysisException(string message)
        : base(message)
    {
    }

    public VisionAnalysisException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override int StatusCode => (int)HttpStatusCode.BadGateway; // 502

    public override string Title => "Vision analysis unavailable";
}
