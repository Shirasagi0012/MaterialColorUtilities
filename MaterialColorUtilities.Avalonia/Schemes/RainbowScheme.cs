namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class RainbowScheme : SchemeProviderBase
{
    public RainbowScheme() : base()
    {
    }

    public RainbowScheme(IBinding binding) : base(binding)
    {
    }

    public RainbowScheme(Color color) : base(color)
    {
    }

    public RainbowScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeRainbow(seedHct, isDark, contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}