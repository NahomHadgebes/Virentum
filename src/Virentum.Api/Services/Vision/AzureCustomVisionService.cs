using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Exceptions;
using Virentum.Api.Options;

namespace Virentum.Api.Services.Vision;

/// <summary>
/// Azure Custom Vision implementation of <see cref="IVisionService"/>.
///
/// In stub mode (the default for local development, controlled via
/// <see cref="CustomVisionOptions.UseStub"/>) it returns a deterministic
/// prediction derived from a hash of the image bytes, so the full pipeline is
/// exercisable without provisioned cloud credentials. With stub mode disabled it
/// calls the real Custom Vision prediction endpoint via an injected, typed
/// <see cref="HttpClient"/>.
/// </summary>
public sealed class AzureCustomVisionService : IVisionService
{
    private readonly HttpClient _httpClient;
    private readonly CustomVisionOptions _options;
    private readonly ILogger<AzureCustomVisionService> _logger;

    public AzureCustomVisionService(
        HttpClient httpClient,
        IOptions<CustomVisionOptions> options,
        ILogger<AzureCustomVisionService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<VisionPrediction> AnalyseAsync(
        SupportedFruit fruit,
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        if (_options.UseStub)
        {
            return BuildDeterministicStub(fruit, imageBytes);
        }

        try
        {
            return await CallCustomVisionAsync(fruit, imageBytes, cancellationToken);
        }
        catch (Exception ex) when (ex is not VisionAnalysisException and not OperationCanceledException)
        {
            // Structured log keeps the full detail server-side; the thrown
            // domain exception carries only a client-safe message.
            _logger.LogError(
                ex,
                "Custom Vision call failed for {Fruit} (project {ProjectId}, model {Model})",
                fruit,
                _options.ProjectId,
                _options.PublishedModelName);

            throw new VisionAnalysisException(
                "The produce-vision service is temporarily unavailable.", ex);
        }
    }

    private async Task<VisionPrediction> CallCustomVisionAsync(
        SupportedFruit fruit,
        byte[] imageBytes,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"customvision/v3.0/Prediction/{_options.ProjectId}/classify/iterations/" +
            $"{_options.PublishedModelName}/image";

        using var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content,
        };
        request.Headers.Add("Prediction-Key", _options.PredictionKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new VisionAnalysisException(
                $"Custom Vision returned status {(int)response.StatusCode}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return MapPrediction(fruit, document);
    }

    /// <summary>
    /// Reduces the model's tag probabilities to a single ripeness score. Tags
    /// whose names indicate advanced ripeness contribute positively; the highest
    /// matching probability wins, falling back to the dominant tag's probability.
    /// </summary>
    private static VisionPrediction MapPrediction(SupportedFruit fruit, JsonDocument document)
    {
        if (!document.RootElement.TryGetProperty("predictions", out var predictions)
            || predictions.ValueKind != JsonValueKind.Array
            || predictions.GetArrayLength() == 0)
        {
            throw new VisionAnalysisException("Custom Vision response contained no predictions.");
        }

        var tags = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        double ripenessScore = 0d;

        foreach (var prediction in predictions.EnumerateArray())
        {
            var tagName = prediction.GetProperty("tagName").GetString() ?? string.Empty;
            var probability = prediction.GetProperty("probability").GetDouble();
            tags[tagName] = probability;

            if (IsRipenessTag(tagName))
            {
                ripenessScore = Math.Max(ripenessScore, probability);
            }
        }

        return new VisionPrediction(fruit, Math.Clamp(ripenessScore, 0d, 1d), tags);
    }

    private static bool IsRipenessTag(string tagName) =>
        tagName.Contains("ripe", StringComparison.OrdinalIgnoreCase)
        || tagName.Contains("spoil", StringComparison.OrdinalIgnoreCase)
        || tagName.Contains("brown", StringComparison.OrdinalIgnoreCase);

    private static VisionPrediction BuildDeterministicStub(SupportedFruit fruit, byte[] imageBytes)
    {
        // Hash the image so the same photo always yields the same score — useful
        // for predictable demos and integration tests.
        var hash = SHA256.HashData(imageBytes);
        var score = (hash[0] << 8 | hash[1]) / 65535d; // 0.0 – 1.0

        var tags = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["ripe"] = score,
            ["unripe"] = 1d - score,
        };

        return new VisionPrediction(fruit, score, tags);
    }
}
