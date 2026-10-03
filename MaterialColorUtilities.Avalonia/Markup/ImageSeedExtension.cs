using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia.Markup;

/// <summary>One-way binding to a ranked candidate in a shared image color result.</summary>
/// <remarks>
/// This extension only reads <see cref="ImageColorSource.Result"/>; it never extracts image colors.
/// Missing candidates use <see cref="FallbackColor"/>, then the result's nullable seed.
/// </remarks>
public sealed class ImageSeedExtension
{
    private readonly ImageColorSource _source;

    // StaticResource has the static return type object in compiled Avalonia XAML.
    // A strongly typed constructor prevents nested {StaticResource ...} arguments from compiling.
    public ImageSeedExtension(object source)
    {
        _source = source as ImageColorSource ?? throw new ArgumentException(
            "The image seed source must be an ImageColorSource.", nameof(source));
    }

    /// <summary>Zero-based candidate rank. Zero selects the highest-scoring candidate.</summary>
    public int Index { get; set; }

    /// <summary>An optional opaque fallback used when the requested candidate is absent.</summary>
    public Color? FallbackColor { get; set; }

    public ReflectionBinding ProvideValue(IServiceProvider serviceProvider)
    {
        if (Index < 0)
            throw new ArgumentOutOfRangeException(nameof(Index), Index, "The candidate index must be non-negative.");
        if (FallbackColor is { A: not 255 })
            throw new ArgumentException("The fallback color must be opaque.", nameof(FallbackColor));

        return new ReflectionBinding(nameof(ImageColorSource.Result))
        {
            Source = _source,
            Mode = BindingMode.OneWay,
            Converter = new CandidateConverter(Index, FallbackColor)
        };
    }

    private sealed class CandidateConverter(int index, Color? fallbackColor) : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not ImageColorResult result)
                return fallbackColor;

            return index < result.Candidates.Count ? result.Candidates[index] : fallbackColor ?? result.SeedColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException("An image seed binding is one-way.");
    }
}
