using Virentum.Api.Contracts.Requests;
using Virentum.Api.Contracts.Responses;
using Virentum.Api.Domain.Models;
using Virentum.Api.Domain.Processors;
using Virentum.Api.Exceptions;
using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Infrastructure.Persistence.Repositories;
using Virentum.Api.Services.Vision;

namespace Virentum.Api.Services.Inspection;

/// <summary>
/// Orchestrates the inspection pipeline. It owns no fruit-specific logic — that
/// is delegated to the processor resolved from <see cref="IFruitProcessorFactory"/>
/// — and no persistence detail, which sits behind <see cref="IInspectionRepository"/>.
/// </summary>
public sealed class InspectionService : IInspectionService
{
    // Reject anything larger than this before touching the vision provider.
    private const int MaxImageBytes = 8 * 1024 * 1024; // 8 MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
    };

    private readonly IVisionService _vision;
    private readonly IFruitProcessorFactory _processorFactory;
    private readonly IInspectionRepository _repository;
    private readonly TimeProvider _clock;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(
        IVisionService vision,
        IFruitProcessorFactory processorFactory,
        IInspectionRepository repository,
        TimeProvider clock,
        ILogger<InspectionService> logger)
    {
        _vision = vision;
        _processorFactory = processorFactory;
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    public async Task<InspectionResponse> ScanAsync(
        ScanRequest request,
        string storeId,
        CancellationToken cancellationToken = default)
    {
        var imageBytes = await ReadAndValidateImageAsync(request, cancellationToken);

        // 1. Vision: vendor-neutral prediction.
        var prediction = await _vision.AnalyseAsync(request.FruitType, imageBytes, cancellationToken);

        // 2. Strategy: the factory yields the right processor with zero branching here.
        var processor = _processorFactory.Create(request.FruitType);
        RipenessAssessment assessment = processor.Assess(prediction);

        // The operator declares the fruit; nothing in the pipeline identifies
        // it. This is the one contradiction the colour data can actually catch.
        var colourMismatch = processor.DescribeColourMismatch(prediction);
        if (colourMismatch is not null)
        {
            _logger.LogInformation(
                "Colour mismatch for store {StoreId}: {Mismatch}", storeId, colourMismatch);
        }

        // 3. Persist (entity stays inside this layer).
        var scannedAt = _clock.GetUtcNow();
        var record = new InspectionRecord
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            FruitType = request.FruitType,
            RipenessPercent = assessment.RipenessPercent,
            CommercialStatus = assessment.CommercialStatus,
            Recommendation = assessment.Recommendation,
            ScannedAt = scannedAt,
        };
        await _repository.AddAsync(record, cancellationToken);

        _logger.LogInformation(
            "Inspection completed for store {StoreId}: {Fruit} at {Ripeness}% ⇒ {Status}",
            storeId,
            request.FruitType,
            assessment.RipenessPercent,
            assessment.CommercialStatus);

        // 4. Map entity/domain to the client DTO.
        return new InspectionResponse(
            record.FruitType,
            record.RipenessPercent,
            record.CommercialStatus,
            record.Recommendation,
            record.ScannedAt,
            colourMismatch);
    }

    public async Task<IReadOnlyList<InspectionHistoryItem>> GetHistoryAsync(
        string storeId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var records = await _repository.GetRecentByStoreAsync(storeId, limit, cancellationToken);

        return records
            .Select(record => new InspectionHistoryItem(
                record.Id,
                record.FruitType,
                record.RipenessPercent,
                record.CommercialStatus,
                record.Recommendation,
                record.ScannedAt))
            .ToList();
    }

    public async Task<InspectionSummaryResponse> GetSummaryAsync(
        string storeId,
        int days,
        CancellationToken cancellationToken = default)
    {
        var since = _clock.GetUtcNow().AddDays(-days);
        var statistics = await _repository.GetStatisticsSinceAsync(storeId, since, cancellationToken);

        return new InspectionSummaryResponse(
            days,
            since,
            statistics.TotalScans,
            ZeroFilled(statistics.CountByStatus, (status, count) => new StatusCount(status, count)),
            ZeroFilled(statistics.CountByFruit, (fruit, count) => new FruitCount(fruit, count)),
            statistics.AverageRipenessPercent,
            statistics.LastScanAt);
    }

    /// <summary>
    /// Projects a sparse count map onto every member of the enum, in declaration
    /// order. The client charts a fixed set of categories, so absence has to be
    /// an explicit zero rather than a missing key.
    /// </summary>
    private static IReadOnlyList<TResult> ZeroFilled<TEnum, TResult>(
        IReadOnlyDictionary<TEnum, int> counts,
        Func<TEnum, int, TResult> project)
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(member => project(member, counts.TryGetValue(member, out var count) ? count : 0))
            .ToList();

    private static async Task<byte[]> ReadAndValidateImageAsync(
        ScanRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Image is null || request.Image.Length == 0)
        {
            throw new InvalidInspectionRequestException("An image file is required.");
        }

        if (request.Image.Length > MaxImageBytes)
        {
            throw new InvalidInspectionRequestException(
                $"Image exceeds the {MaxImageBytes / (1024 * 1024)} MB limit.");
        }

        if (!AllowedContentTypes.Contains(request.Image.ContentType))
        {
            throw new InvalidInspectionRequestException(
                $"Unsupported image content type '{request.Image.ContentType}'.");
        }

        using var buffer = new MemoryStream();
        await request.Image.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
