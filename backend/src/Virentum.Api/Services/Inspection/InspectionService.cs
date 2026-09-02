using Microsoft.AspNetCore.Http;
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

    /// <summary>
    /// More angles is more evidence, but the pooled reading stops improving
    /// long before the upload cost does. Three is enough for skin, a second
    /// angle and the inside.
    /// </summary>
    private const int MaxImages = 3;

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
        var images = await ReadAndValidateImagesAsync(request, cancellationToken);

        // 1. Vision: vendor-neutral prediction, pooled across every photograph.
        var prediction = await _vision.AnalyseAsync(request.FruitType, images, cancellationToken);

        // 2. Strategy: the factory yields the right processor with zero branching here.
        var processor = _processorFactory.Create(request.FruitType);
        RipenessAssessment assessment = processor.Assess(prediction, request.Audience);

        // The colour stage is a heuristic. What it had to work with travels with
        // the result so the client never presents a thin reading as a finding.
        var evidence = processor.AssessEvidence(prediction);
        if (!evidence.IsReliable)
        {
            _logger.LogInformation(
                "Unreliable reading for store {StoreId}: {Concerns}",
                storeId,
                string.Join(" | ", evidence.Concerns));
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
            "Inspection completed for store {StoreId}: {Fruit} at {Ripeness}% over {Images} image(s) => {Status}",
            storeId,
            request.FruitType,
            assessment.RipenessPercent,
            images.Count,
            assessment.CommercialStatus);

        // 4. Map to the client DTO.
        return new InspectionResponse(
            record.FruitType,
            request.Audience,
            assessment.RipenessPercent,
            assessment.StageName,
            assessment.Appearance,
            assessment.CommercialStatus,
            assessment.Edibility,
            assessment.Recommendation,
            assessment.Factors
                .Select(factor => new AnalysisFactorResponse(factor.Label, factor.Share, factor.Meaning))
                .ToList(),
            prediction.ImageCount,
            record.ScannedAt,
            new InspectionEvidenceResponse(evidence.IsReliable, evidence.Concerns));
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

    /// <summary>
    /// Validates every supplied photograph before any of them reaches the vision
    /// provider. One bad file fails the whole scan rather than being dropped
    /// quietly: an operator who attached three pictures should not receive a
    /// verdict silently based on two.
    /// </summary>
    private static async Task<IReadOnlyList<byte[]>> ReadAndValidateImagesAsync(
        ScanRequest request,
        CancellationToken cancellationToken)
    {
        var files = request.Images;

        if (files is null || files.Count == 0)
        {
            throw new InvalidInspectionRequestException("At least one image is required.");
        }

        if (files.Count > MaxImages)
        {
            throw new InvalidInspectionRequestException(
                $"At most {MaxImages} images can be analysed together; {files.Count} were sent.");
        }

        var images = new List<byte[]>(files.Count);
        foreach (var file in files)
        {
            images.Add(await ReadAndValidateImageAsync(file, cancellationToken));
        }

        return images;
    }

    private static async Task<byte[]> ReadAndValidateImageAsync(
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            throw new InvalidInspectionRequestException("An image file is required.");
        }

        if (image.Length > MaxImageBytes)
        {
            throw new InvalidInspectionRequestException(
                $"Image exceeds the {MaxImageBytes / (1024 * 1024)} MB limit.");
        }

        if (!AllowedContentTypes.Contains(image.ContentType))
        {
            throw new InvalidInspectionRequestException(
                $"Unsupported image content type '{image.ContentType}'.");
        }

        using var buffer = new MemoryStream();
        await image.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
