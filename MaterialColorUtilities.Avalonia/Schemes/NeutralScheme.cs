namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class NeutralScheme : SchemeProviderBase
{
    public NeutralScheme() : base()
    {
    }

    public NeutralScheme(IBinding binding) : base(binding)
    {
    }

    public NeutralScheme(Color color) : base(color)
    {
    }

    public NeutralScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var specVersion = ResolveSpecVersion();
        var platform = ResolvePlatform();
        var isDark = IsDark(theme);

        return new SchemeNeutral(seedHct, isDark, contrast, specVersion, platform);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
