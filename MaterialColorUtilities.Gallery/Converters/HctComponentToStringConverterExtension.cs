using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Gallery.Controls;
using static MaterialColorUtilities.Gallery.Controls.HctColorSlider;

namespace MaterialColorUtilities.Gallery.Converters;

public class HctComponentToStringConverterExtension : MarkupExtension
{
    private const double MaxHue = 359.0;
    private const double MaxTone = 100.0;

    private readonly HctComponent? _component;
    private HctSelection _lastHct = new(0.0, 0.0, 0.0);

    private readonly FuncValueConverter<HctSelection, string> _hueConverter;
    private readonly FuncValueConverter<HctSelection, string> _chromaConverter;
    private readonly FuncValueConverter<HctSelection, string> _toneConverter;
    private readonly FuncValueConverter<HctSelection, string> _hctConverter;

    public HctComponentToStringConverterExtension()
    {
        _hueConverter = new(
            convert: hct =>
            {
                _lastHct = hct;
                return hct.Hue.ToString("F1", CultureInfo.CurrentCulture);
            },
            convertBack: value => TryParseNumber(value, out var hue)
                ? UpdateComponent(_lastHct, HctComponent.Hue, hue)
                : _lastHct
        );

        _chromaConverter = new(
            convert: hct =>
            {
                _lastHct = hct;
                return hct.Chroma.ToString("F1", CultureInfo.CurrentCulture);
            },
            convertBack: value => TryParseNumber(value, out var chroma)
                ? UpdateComponent(_lastHct, HctComponent.Chroma, chroma)
                : _lastHct
        );

        _toneConverter = new(
            convert: hct =>
            {
                _lastHct = hct;
                return hct.Tone.ToString("F1", CultureInfo.CurrentCulture);
            },
            convertBack: value => TryParseNumber(value, out var tone)
                ? UpdateComponent(_lastHct, HctComponent.Tone, tone)
                : _lastHct
        );

        _hctConverter = new(
            convert: hct =>
            {
                _lastHct = hct;
                return string.Create(
                    CultureInfo.CurrentCulture,
                    $"{hct.Hue:F1}, {hct.Chroma:F1}, {hct.Tone:F1}");
            },
            convertBack: value => TryParseHct(value, out var hue, out var chroma, out var tone)
                ? CreateSafeHct(hue, chroma, tone)
                : _lastHct
        );
    }

    public HctComponentToStringConverterExtension(HctComponent component)
        : this()
    {
        _component = component;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        _component switch
        {
            HctComponent.Hue => _hueConverter,
            HctComponent.Chroma => _chromaConverter,
            HctComponent.Tone => _toneConverter,
            _ => _hctConverter
        };

    private static HctSelection UpdateComponent(HctSelection source, HctComponent component, double value)
    {
        var hue = Math.Clamp(source.Hue, 0.0, MaxHue);
        var tone = Math.Clamp(source.Tone, 0.0, MaxTone);
        var chroma = Math.Max(0.0, source.Chroma);

        switch (component)
        {
            case HctComponent.Hue:
                hue = Math.Clamp(value, 0.0, MaxHue);
                chroma = Math.Min(chroma, GetDynamicMaxChroma(hue, tone));
                break;
            case HctComponent.Chroma:
                chroma = Math.Clamp(value, 0.0, GetDynamicMaxChroma(hue, tone));
                break;
            case HctComponent.Tone:
                tone = Math.Clamp(value, 0.0, MaxTone);
                chroma = Math.Min(chroma, GetDynamicMaxChroma(hue, tone));
                break;
        }

        return new HctSelection(hue, chroma, tone).Normalize();
    }

    private static HctSelection CreateSafeHct(double hue, double chroma, double tone)
    {
        var safeHue = Math.Clamp(hue, 0.0, MaxHue);
        var safeTone = Math.Clamp(tone, 0.0, MaxTone);
        var safeChroma = Math.Clamp(chroma, 0.0, GetDynamicMaxChroma(safeHue, safeTone));

        return new HctSelection(safeHue, safeChroma, safeTone).Normalize();
    }

    private static bool TryParseNumber(string? value, out double result)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
        {
            return true;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseHct(string? value, out double hue, out double chroma, out double tone)
    {
        hue = 0.0;
        chroma = 0.0;
        tone = 0.0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        return TryParseNumber(parts[0], out hue)
               && TryParseNumber(parts[1], out chroma)
               && TryParseNumber(parts[2], out tone);
    }
}
