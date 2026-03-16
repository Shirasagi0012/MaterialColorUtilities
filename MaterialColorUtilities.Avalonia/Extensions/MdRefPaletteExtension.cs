using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia.Helpers;
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

    public MdRefPaletteExtension(RefPaletteToken palette, byte tone, string customKey)
    {
        if (!TokenHelper.IsCustom(palette))
            throw new ArgumentException($"The token '{palette}' does not support a custom key.", nameof(palette));

        Palette = palette;
        Tone = tone;
        CustomKey = customKey;
    }

    [ConstructorArgument("customKey")] public string? CustomKey { get; set; }

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
        var normalizedKey = CustomKey?.Trim();

        return ShouldProvideBrush(target)
            ? ProvideRefColorBinding(parentStack, Palette, Tone, CustomKey)
                .Select(IBrush (color) => new SolidColorBrush(color))
                .ToBinding()
            : ProvideRefColorBinding(parentStack, Palette, Tone, CustomKey)
                .ToBinding();
    }
}
