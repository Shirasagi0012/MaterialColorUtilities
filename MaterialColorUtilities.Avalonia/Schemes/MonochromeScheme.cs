using Avalonia.Styling;
using MaterialColorUtilities.Scheme;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;

public sealed class MonochromeScheme : SchemeProviderBase
{
    public MonochromeScheme() : base()
    {
    }

    public MonochromeScheme(IBinding binding) : base(binding)
    {
    }

    public MonochromeScheme(Color color) : base(color)
    {
    }

    public MonochromeScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeMonochrome(seedHct, isDark, contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}