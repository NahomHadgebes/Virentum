using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Models;
using Virentum.Api.Exceptions;

namespace Virentum.Api.Services.Vision;

public sealed class ColorHeuristicVisionService : IVisionService
{
    // Downscale before analysis so cost is bounded regardless of upload size.
    private const int MaxAnalysisDimension = 160;

    // Pixels below this saturation are treated as background (white/grey/black).
    private const float BackgroundSaturationFloor = 0.15f;

    // Ripeness anchors for each colour bucket on the 0 (unripe) – 1 (spoiled) scale.
    private const double GreenAnchor = 0.10;
    private const double YellowAnchor = 0.55;
    private const double BrownAnchor = 0.95;

    private readonly ILogger<ColorHeuristicVisionService> _logger;

    public ColorHeuristicVisionService(ILogger<ColorHeuristicVisionService> logger)
    {
        _logger = logger;
    }

    public Task<VisionPrediction> AnalyseAsync(
        SupportedFruit fruit,
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var image = Image.Load<Rgb24>(imageBytes);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxAnalysisDimension, MaxAnalysisDimension),
            }));

            long green = 0, yellow = 0, brown = 0;

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    foreach (ref readonly var px in accessor.GetRowSpan(y))
                    {
                        var (h, s, v) = RgbToHsv(px.R, px.G, px.B);

                        if (s < BackgroundSaturationFloor)
                        {
                            continue; // background-ish pixel
                        }

                        if (v < 0.30f || (h >= 12f && h < 45f && v < 0.65f))
                        {
                            brown++;            // dark or brown ⇒ overripe
                        }
                        else if (h >= 42f && h < 72f)
                        {
                            yellow++;           // yellow ⇒ ripe
                        }
                        else if (h >= 70f && h < 175f)
                        {
                            green++;            // green ⇒ unripe
                        }
                    }
                }
            });

            var classified = green + yellow + brown;
            if (classified == 0)
            {
                _logger.LogWarning(
                    "Colour analysis found no fruit-like pixels for {Fruit}; defaulting to mid ripeness.",
                    fruit);
                return Task.FromResult(new VisionPrediction(
                    fruit, 0.5d, new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)));
            }

            double greenFrac = (double)green / classified;
            double yellowFrac = (double)yellow / classified;
            double brownFrac = (double)brown / classified;

            var score = Math.Clamp(
                greenFrac * GreenAnchor + yellowFrac * YellowAnchor + brownFrac * BrownAnchor,
                0d, 1d);

            var tags = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["green"] = greenFrac,
                ["yellow"] = yellowFrac,
                ["brownDark"] = brownFrac,
            };

            _logger.LogInformation(
                "Colour analysis for {Fruit}: green={Green:P0} yellow={Yellow:P0} brown/dark={Brown:P0} => score {Score:F2}",
                fruit, greenFrac, yellowFrac, brownFrac, score);

            return Task.FromResult(new VisionPrediction(fruit, score, tags));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to analyse uploaded image for {Fruit}.", fruit);
            throw new VisionAnalysisException("The uploaded image could not be analysed.", ex);
        }
    }

    /// <summary>Converts an sRGB pixel to HSV (H in degrees 0–360, S/V in 0–1).</summary>
    private static (float H, float S, float V) RgbToHsv(byte r, byte g, byte b)
    {
        float rf = r / 255f, gf = g / 255f, bf = b / 255f;
        float max = Math.Max(rf, Math.Max(gf, bf));
        float min = Math.Min(rf, Math.Min(gf, bf));
        float delta = max - min;

        float h = 0f;
        if (delta > 0f)
        {
            if (max == rf) h = 60f * (((gf - bf) / delta) % 6f);
            else if (max == gf) h = 60f * (((bf - rf) / delta) + 2f);
            else h = 60f * (((rf - gf) / delta) + 4f);
        }
        if (h < 0f) h += 360f;

        float s = max <= 0f ? 0f : delta / max;
        return (h, s, max);
    }
}
