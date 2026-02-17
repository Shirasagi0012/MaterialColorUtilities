namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class ExpressiveScheme : SchemeProviderBase
{
    public ExpressiveScheme() : base()
    {
    }

    public ExpressiveScheme(IBinding binding) : base(binding)
    {
    }

    public ExpressiveScheme(Color color) : base(color)
    {
    }

    public ExpressiveScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeExpressive(seedHct, isDark, contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}