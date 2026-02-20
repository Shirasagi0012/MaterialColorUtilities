namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class FidelityScheme : SchemeProviderBase
{
    public FidelityScheme() : base()
    {
    }

    public FidelityScheme(IBinding binding) : base(binding)
    {
    }

    public FidelityScheme(Color color) : base(color)
    {
    }

    public FidelityScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeFidelity(seedHct, isDark, contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}