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
    public TonalSpotScheme() : base()
    {
    }

    public TonalSpotScheme(IBinding binding) : base(binding)
    {
    }

    public TonalSpotScheme(string color) : base(color)
    {
    }

    public TonalSpotScheme(Color color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var specVersion = ResolveSpecVersion();
        var platform = ResolvePlatform();
        var isDark = IsDark(theme);

        return new SchemeTonalSpot(seedHct, isDark, contrast, specVersion, platform);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
