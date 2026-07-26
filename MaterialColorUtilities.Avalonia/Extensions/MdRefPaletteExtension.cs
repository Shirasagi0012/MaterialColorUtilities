using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia;

public class MdRefPaletteExtension
{
    public MdRefPaletteExtension(RefPaletteToken palette, byte tone)
    {
        Palette = palette;
        Tone = tone;
    }

    [ConstructorArgument("palette")]
    public RefPaletteToken Palette { get; set; }

    [ConstructorArgument("tone")]
    public byte Tone
    {
        get;
        set => field = value <= 100
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), "Tone must be in range 0..100.");
    } = 40;

    public ThemeVariant? Theme { get; set; }

    /// <summary>
    /// Names the <see cref="CustomColor"/> whose tonal palette to read. Required for
    /// <see cref="RefPaletteToken.Custom"/>, and ignored for every other palette.
    /// </summary>
    public string? CustomKey { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var observable = TokenExtensionHelper<Color, RefPaletteTokenKey, MaterialColorSchemeHost>.ProvideObservable(
            serviceProvider,
            new TokenKey<Color, RefPaletteTokenKey>(new RefPaletteTokenKey(Palette, Tone, CustomKey)),
            Theme,
            Colors.Transparent);


        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target
            && MaterialMarkupExtensionHelper.ShouldProvideBrush(target))
            return new ColorToBrushObservable(observable)
                .ToBinding();

        return observable
            .ToBinding();
    }
}
