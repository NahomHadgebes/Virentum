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
            record.ScannedAt);
    }

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
