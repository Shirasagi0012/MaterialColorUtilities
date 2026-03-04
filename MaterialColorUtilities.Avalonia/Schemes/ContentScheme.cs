namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class ContentScheme : SchemeProviderBase
{
    public ContentScheme() : base()
    {
    }

    public ContentScheme(IBinding binding) : base(binding)
    {
    }

    public ContentScheme(Color color) : base(color)
    {
    }

    public ContentScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var specVersion = ResolveSpecVersion();
        var platform = ResolvePlatform();
        var isDark = IsDark(theme);

        return new SchemeContent(seedHct, isDark, contrast, specVersion, platform);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
