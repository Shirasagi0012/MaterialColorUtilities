namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class FruitSaladScheme : SchemeProviderBase
{
    public FruitSaladScheme() : base()
    {
    }

    public FruitSaladScheme(IBinding binding) : base(binding)
    {
    }

    public FruitSaladScheme(Color color) : base(color)
    {
    }

    public FruitSaladScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeFruitSalad(seedHct, isDark, contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}