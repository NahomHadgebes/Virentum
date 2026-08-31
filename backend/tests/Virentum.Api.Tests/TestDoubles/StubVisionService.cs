using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Services.Vision;

namespace Virentum.Api.Tests.TestDoubles;

/// <summary>
/// Returns a fixed ripeness score so a test can place the prediction anywhere on
/// the scale without depending on real image analysis. Hand-written rather than
/// generated: the whole implementation is two lines and reads at a glance.
/// </summary>
internal sealed class StubVisionService : IVisionService
{
    private readonly double _ripenessScore;

    public StubVisionService(double ripenessScore)
    {
        _ripenessScore = ripenessScore;
    }

    /// <summary>Set when AnalyseAsync is reached, so a test can assert it was not.</summary>
    public bool WasCalled { get; private set; }

    public Task<VisionPrediction> AnalyseAsync(
        SupportedFruit fruit,
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        return Task.FromResult(
            new VisionPrediction(fruit, _ripenessScore, new Dictionary<string, double>()));
    }
}
