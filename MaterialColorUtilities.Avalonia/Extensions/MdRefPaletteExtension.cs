using System;
using Avalonia.Data;
using Avalonia.Metadata;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia;

/// <summary>
/// Resolves a tone from one of the reference tonal palettes of the scheme in scope at the target
/// element. Reference palettes derive from the seed color alone, so they do not vary with the
/// theme variant.
/// </summary>
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

    /// <summary>
    /// Names the <see cref="CustomColor"/> whose tonal palette to read. Required for
    /// <see cref="RefPaletteToken.Custom"/>, and ignored for every other palette.
    /// </summary>
    public string? CustomKey { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return ColorTokenBinding.RefPalette(
            Palette,
            Tone,
            CustomKey,
            MdSysColorExtension.ShouldProvideBrush(serviceProvider));
    }
}
