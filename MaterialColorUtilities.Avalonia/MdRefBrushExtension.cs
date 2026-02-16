using System;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Avalonia.Internal;

namespace MaterialColorUtilities.Avalonia;

public class MdRefBrushExtension
{
    public MdRefBrushExtension()
    {
    }

    public MdRefBrushExtension(RefPaletteToken palette, byte tone)
    {
        Palette = palette;
        Tone = tone;
    }

    [ConstructorArgument("palette")] public RefPaletteToken Palette { get; set; }

    [ConstructorArgument("tone")]
    public byte Tone
    {
        get;
        set => field = value <= 100
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), "Tone must be in range 0..100.");
    } = 40;

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return MaterialColorRuntime.ProvideRefBrushBinding(serviceProvider, Palette, Tone);
    }
}