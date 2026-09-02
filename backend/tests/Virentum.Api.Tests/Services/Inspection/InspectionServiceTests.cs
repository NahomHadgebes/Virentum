using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Virentum.Api.Contracts.Requests;
using Virentum.Api.Contracts.Responses;
using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Virentum.Api.Exceptions;
using Virentum.Api.Services.Inspection;
using Virentum.Api.Services.Vision;
using Virentum.Api.Tests.TestDoubles;
using Xunit;

namespace Virentum.Api.Tests.Services.Inspection;

/// <summary>
/// The request-validation rules here are the same ones the frontend mirrors in
/// validation/image.ts. Asserting them from both sides is deliberate: if the
/// ceiling or the content-type list moves, one of the two suites breaks.
/// </summary>
public sealed class InspectionServiceTests
{
    private const int EightMegabytes = 8 * 1024 * 1024;
    private static readonly DateTimeOffset ScanInstant =
        new(2026, 8, 31, 14, 30, 0, TimeSpan.Zero);

    private readonly RecordingInspectionRepository _repository = new();

    /// <summary>
    /// FormFile.Length is taken from the constructor argument, which is exactly
    /// what the service checks, so an oversized file needs no oversized buffer.
    /// </summary>
    private static IFormFile FileOf(string contentType, long length = 1024, string name = "produce.png")
    {
        // Oversized requests are rejected before the copy, so only a file that
        // should be accepted needs its bytes actually present.
        var stream = new MemoryStream(new byte[length <= EightMegabytes ? length : 1024]);
        return new FormFile(stream, 0, length, "Image", name)
        {
            Headers = new HeaderDictionary { { "Content-Type", contentType } },
        };
    }

    private InspectionService CreateService(IVisionService vision) =>
        new(
            vision,
            new FruitProcessorFactory(new IFruitProcessor[] { new BananaProcessor(), new AvocadoProcessor() }),
            _repository,
            new FixedTimeProvider(ScanInstant),
            NullLogger<InspectionService>.Instance);

    private Task<InspectionResponse> ScanAsync(
        IFormFile? image,
        SupportedFruit fruit = SupportedFruit.Banana,
        double ripenessScore = 0.5,
        IVisionService? vision = null)
    {
        var service = CreateService(vision ?? new StubVisionService(ripenessScore));
        return service.ScanAsync(new ScanRequest { Image = image, FruitType = fruit }, "demo-store");
    }

    [Fact]
    public async Task Rejects_a_missing_image()
    {
        var exception = await Assert.ThrowsAsync<InvalidInspectionRequestException>(
            () => ScanAsync(image: null));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("An image file is required.", exception.Message);
    }

    [Fact]
    public async Task Rejects_an_empty_image_with_the_same_message_as_a_missing_one()
    {
        var exception = await Assert.ThrowsAsync<InvalidInspectionRequestException>(
            () => ScanAsync(FileOf("image/png", length: 0)));

        Assert.Equal("An image file is required.", exception.Message);
    }

    [Fact]
    public async Task Accepts_an_image_at_exactly_the_eight_megabyte_ceiling()
    {
        var response = await ScanAsync(FileOf("image/png", EightMegabytes));

        Assert.Equal(SupportedFruit.Banana, response.FruitType);
    }

    [Fact]
    public async Task Rejects_an_image_one_byte_over_the_ceiling()
    {
        var exception = await Assert.ThrowsAsync<InvalidInspectionRequestException>(
            () => ScanAsync(FileOf("image/png", EightMegabytes + 1)));

        Assert.Equal("Image exceeds the 8 MB limit.", exception.Message);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public async Task Accepts_every_allowed_content_type(string contentType)
    {
        var response = await ScanAsync(FileOf(contentType));

        Assert.Equal(50, response.RipenessPercent);
    }

    [Fact]
    public async Task Matches_the_content_type_without_regard_to_case()
    {
        var response = await ScanAsync(FileOf("IMAGE/PNG"));

        Assert.Equal(50, response.RipenessPercent);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    [InlineData("image/svg+xml")]
    [InlineData("image/heic")]
    public async Task Rejects_a_content_type_outside_the_allow_list(string contentType)
    {
        var exception = await Assert.ThrowsAsync<InvalidInspectionRequestException>(
            () => ScanAsync(FileOf(contentType)));

        Assert.Equal($"Unsupported image content type '{contentType}'.", exception.Message);
    }

    [Fact]
    public async Task Does_not_call_the_vision_provider_for_an_invalid_request()
    {
        var vision = new StubVisionService(0.5);

        await Assert.ThrowsAsync<InvalidInspectionRequestException>(
            () => ScanAsync(FileOf("application/pdf"), vision: vision));

        Assert.False(vision.WasCalled);
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task Returns_the_assessment_the_processor_produced()
    {
        var response = await ScanAsync(FileOf("image/png"), SupportedFruit.Banana, ripenessScore: 0.81);

        Assert.Equal(SupportedFruit.Banana, response.FruitType);
        Assert.Equal(81, response.RipenessPercent);
        Assert.Equal(CommercialStatus.ActionRequired, response.CommercialStatus);
        Assert.Equal(ScanInstant, response.ScannedAt);
    }

    /// <summary>
    /// The same score must route through the fruit's own processor. 0.80 is
    /// ActionRequired for a banana and ReadyForSale for an avocado.
    /// </summary>
    [Fact]
    public async Task Routes_to_the_processor_for_the_requested_fruit()
    {
        var banana = await ScanAsync(FileOf("image/png"), SupportedFruit.Banana, 0.80);
        var avocado = await ScanAsync(FileOf("image/png"), SupportedFruit.Avocado, 0.80);

        Assert.Equal(CommercialStatus.ActionRequired, banana.CommercialStatus);
        Assert.Equal(CommercialStatus.ReadyForSale, avocado.CommercialStatus);
    }

    [Fact]
    public async Task Persists_the_inspection_against_the_calling_store()
    {
        var response = await ScanAsync(FileOf("image/png"), SupportedFruit.Avocado, 0.60);

        var saved = Assert.Single(_repository.Saved);
        Assert.Equal("demo-store", saved.StoreId);
        Assert.Equal(SupportedFruit.Avocado, saved.FruitType);
        Assert.Equal(response.RipenessPercent, saved.RipenessPercent);
        Assert.Equal(response.CommercialStatus, saved.CommercialStatus);
        Assert.Equal(ScanInstant, saved.ScannedAt);
        Assert.NotEqual(Guid.Empty, saved.Id);
    }

    /// <summary>
    /// The stub reports no colour buckets and no coverage, so nothing limits the
    /// reading and the response says so.
    /// </summary>
    [Fact]
    public async Task Reports_a_reliable_reading_when_nothing_limits_it()
    {
        var response = await ScanAsync(FileOf("image/png"), SupportedFruit.Avocado, 0.5);

        Assert.True(response.Evidence.IsReliable);
    }

    [Fact]
    public async Task Passes_an_unreliable_reading_through_without_blocking_the_assessment()
    {
        var yellowImage = new ColourReportingVisionService(green: 0.1, yellow: 0.85, brownDark: 0.05);

        var response = await ScanAsync(FileOf("image/png"), SupportedFruit.Avocado, vision: yellowImage);

        Assert.False(response.Evidence.IsReliable);
        Assert.Contains(
            response.Evidence.Concerns,
            concern => concern.Contains("Avocado", StringComparison.Ordinal));
        // The scan still produced a verdict and was still recorded.
        Assert.Equal(SupportedFruit.Avocado, response.FruitType);
        Assert.Single(_repository.Saved);
    }

    private sealed class ColourReportingVisionService : IVisionService
    {
        private readonly Dictionary<string, double> _tags;

        public ColourReportingVisionService(double green, double yellow, double brownDark)
        {
            _tags = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                [ColourBuckets.Green] = green,
                [ColourBuckets.Yellow] = yellow,
                [ColourBuckets.BrownDark] = brownDark,
            };
        }

        public Task<VisionPrediction> AnalyseAsync(
            SupportedFruit fruit,
            byte[] imageBytes,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new VisionPrediction(fruit, 0.6, _tags, AnalysedShare: 0.7));
    }

    [Fact]
    public async Task Lets_a_vision_failure_surface_as_a_bad_gateway()
    {
        var exception = await Assert.ThrowsAsync<VisionAnalysisException>(
            () => ScanAsync(FileOf("image/png"), vision: new FailingVisionService()));

        Assert.Equal(502, exception.StatusCode);
        Assert.Empty(_repository.Saved);
    }

    private sealed class FailingVisionService : IVisionService
    {
        public Task<VisionPrediction> AnalyseAsync(
            SupportedFruit fruit,
            byte[] imageBytes,
            CancellationToken cancellationToken = default) =>
            throw new VisionAnalysisException("The vision provider could not be reached.");
    }
}
