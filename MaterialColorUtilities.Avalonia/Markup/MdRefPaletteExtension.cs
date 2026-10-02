using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia.Markup;

/// <summary>Looks up a palette tone through Avalonia's native dynamic resource binding.</summary>
/// <remarks>Use either <see cref="Palette"/> or <see cref="CustomName"/>, never both.</remarks>
public sealed class MdRefPaletteExtension
{
    private RefPaletteToken? _palette;

    public MdRefPaletteExtension() { }

    public MdRefPaletteExtension(RefPaletteToken palette, int tone)
    {
        Palette = palette;
        Tone = tone;
    }

    /// <summary>The standard palette. Defaults to Primary when neither palette selector is supplied.</summary>
    [ConstructorArgument("palette")]
    public RefPaletteToken Palette
    {
        get => _palette ?? RefPaletteToken.Primary;
        set => _palette = value;
    }

    /// <summary>An integer tone from 0 through 100, inclusive.</summary>
    [ConstructorArgument("tone")]
    public int Tone { get; set; }

    /// <summary>The custom palette name, instead of an explicitly supplied <see cref="Palette"/>.</summary>
    public string? CustomName { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (Tone is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Tone), Tone, "Tone must be between 0 and 100.");
        if (CustomName is not null && _palette.HasValue)
            throw new ArgumentException("Specify either Palette or CustomName, not both.", nameof(CustomName));

        var key = CustomName is not null
            ? new RefPaletteKey(CustomName, (byte)Tone)
            : new RefPaletteKey(Palette, (byte)Tone);
        return new DynamicResourceExtension(key).ProvideValue(serviceProvider);
    }
}
