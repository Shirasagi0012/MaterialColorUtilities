using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia;

/// <summary>Bounds sampling, quantization, and candidate selection for an image.</summary>
public sealed record ImageColorOptions
{
    /// <summary>Maximum sampled pixels, from 1 to 65,536. The image is never enlarged.</summary>
    public int MaxSamplePixels { get; init; } = 16_384;

    /// <summary>Maximum quantized colors, from 1 to 256.</summary>
    public int MaxColors { get; init; } = 128;

    /// <summary>Maximum scored candidates, from 1 to 16 and no greater than <see cref="MaxColors"/>.</summary>
    public int DesiredColors { get; init; } = 4;

    /// <summary>Whether Material Score filters low-chroma and uncommon colors.</summary>
    public bool Filter { get; init; } = true;

    /// <summary>An optional opaque seed used only when no candidate remains.</summary>
    public Color? FallbackColor { get; init; }

    internal void Validate()
    {
        if (MaxSamplePixels is < 1 or > 65_536)
            throw new ArgumentOutOfRangeException(nameof(MaxSamplePixels), MaxSamplePixels,
                "The sample limit must be between 1 and 65,536 pixels.");
        if (MaxColors is < 1 or > 256)
            throw new ArgumentOutOfRangeException(nameof(MaxColors), MaxColors,
                "The quantized color limit must be between 1 and 256.");
        if (DesiredColors is < 1 or > 16 || DesiredColors > MaxColors)
            throw new ArgumentOutOfRangeException(nameof(DesiredColors), DesiredColors,
                "The desired color count must be between 1 and 16 and no greater than MaxColors.");
        if (FallbackColor is { A: not 255 })
            throw new ArgumentException("The fallback color must be opaque.", nameof(FallbackColor));
    }
}
