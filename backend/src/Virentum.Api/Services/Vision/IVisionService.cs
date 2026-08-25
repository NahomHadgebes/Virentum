using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;

namespace Virentum.Api.Services.Vision;

/// <summary>
/// Vendor-neutral abstraction over the computer-vision provider. Swapping Azure
/// Custom Vision for another model only requires a new implementation of this
/// interface — callers and processors are unaffected.
/// </summary>
public interface IVisionService
{
    /// <summary>
    /// Analyses the supplied image bytes for the given fruit and returns a
    /// normalised prediction.
    /// </summary>
    /// <exception cref="Exceptions.VisionAnalysisException">
    /// Thrown when the provider cannot be reached or returns an unusable result.
    /// </exception>
    Task<VisionPrediction> AnalyseAsync(
        SupportedFruit fruit,
        byte[] imageBytes,
        CancellationToken cancellationToken = default);
}
