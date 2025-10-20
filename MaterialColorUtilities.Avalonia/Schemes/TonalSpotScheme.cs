using Avalonia.Styling;
using MaterialColorUtilities.Scheme;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;

/// <summary>
/// Provides the default Material tonal spot scheme.
/// </summary>
public sealed class TonalSpotScheme : SchemeProviderBase
{

    public TonalSpotScheme() : base() {}

    public TonalSpotScheme(IBinding binding) : base(binding) {}

    public TonalSpotScheme(string color) : base(color) {}

    public TonalSpotScheme(Color color) : base(color) {}

    override public DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeTonalSpot(sourceColorHct: seedHct, isDark: isDark, contrastLevel: contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider) => this;
}
