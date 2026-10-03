using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia;

/// <summary>An immutable, ranked image palette and its optional best seed.</summary>
public sealed class ImageColorResult
{
    internal ImageColorResult(IEnumerable<Color> candidates, Color? fallbackColor = null)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var copy = candidates.ToArray();
        if (copy.Any(color => color.A != 255))
            throw new ArgumentException("Image candidates must be opaque.", nameof(candidates));
        if (fallbackColor is { A: not 255 })
            throw new ArgumentException("The fallback color must be opaque.", nameof(fallbackColor));

        Candidates = Array.AsReadOnly(copy);
        SeedColor = copy.Length > 0 ? copy[0] : fallbackColor;
        IsFallback = copy.Length == 0 && fallbackColor.HasValue;
    }

    /// <summary>The best candidate, the explicitly configured fallback, or null.</summary>
    public Color? SeedColor { get; }

    /// <summary>Real candidates in Material Score order. Never includes a fallback.</summary>
    public IReadOnlyList<Color> Candidates { get; }

    /// <summary>Whether <see cref="SeedColor"/> comes from an explicitly configured fallback.</summary>
    public bool IsFallback { get; }

    internal static ImageColorResult CreateEmpty(Color? fallbackColor = null) => new([], fallbackColor);

    internal ImageColorResult WithFallback(Color? fallbackColor) =>
        Candidates.Count == 0 ? CreateEmpty(fallbackColor) : this;
}
