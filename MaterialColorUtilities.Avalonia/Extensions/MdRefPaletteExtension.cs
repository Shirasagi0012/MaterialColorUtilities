using System;
using Avalonia.Markup.Xaml;
using static MaterialColorUtilities.Avalonia.Helpers.MaterialColorHelper;

namespace MaterialColorUtilities.Avalonia;

public class MdRefPaletteExtension
{
    public MdRefPaletteExtension()
    {
    }

    public MdRefPaletteExtension(RefPaletteToken palette, byte tone)
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
        var (target, parentStack) = GetContextServices(serviceProvider);

        return ShouldProvideBrush(target)
            ? ProvideRefBrushBinding(target, parentStack, Palette, Tone)
            : ProvideRefColorBinding(target, parentStack, Palette, Tone);
    }
}