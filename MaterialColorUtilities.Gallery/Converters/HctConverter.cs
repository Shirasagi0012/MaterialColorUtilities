using System;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Gallery.Controls;

namespace MaterialColorUtilities.Gallery.Converters;

public class HctToBrushConverter : MarkupExtension
{
    private readonly FuncValueConverter<HctSelection, SolidColorBrush> _converter = new(
        hct => new SolidColorBrush(hct.ToHct().ToAvaloniaColor())
    );

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _converter;
    }
}

public class HctToStringConverter : MarkupExtension
{
    public enum Format
    {
        HctString,
        RgbHex,
        RgbString
    }

    private readonly Format _format;

    public HctToStringConverter(Format format = Format.RgbHex)
    {
        _format = format;
    }

    private readonly FuncValueConverter<HctSelection, string> _hctStringConverter = new(
        hct => $"hct({hct.Hue:F1}°, {hct.Chroma:F1}, {hct.Tone:F1})"
    );

    private readonly FuncValueConverter<HctSelection, string> _rgbHexConverter = new(
        hct => $"#{hct.ToHct().ToAvaloniaColor():X8}"
    );

    private readonly FuncValueConverter<HctSelection, string> _rgbStringConverter = new(
        hct =>
        {
            var color = hct.ToHct().ToAvaloniaColor();
            return $"rgb({color.R}, {color.G}, {color.B})";
        }
    );

    public override object ProvideValue(IServiceProvider serviceProvider) => _format switch
    {
        Format.HctString => _hctStringConverter,
        Format.RgbString => _rgbStringConverter,
        _ => _rgbHexConverter,
    };
}
