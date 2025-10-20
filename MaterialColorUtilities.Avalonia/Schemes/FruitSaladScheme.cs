namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class FruitSaladScheme : SchemeProviderBase
{

    public FruitSaladScheme() : base() {}

    public FruitSaladScheme(IBinding binding) : base(binding) {}

    public FruitSaladScheme(Color color) : base(color) {}

    public FruitSaladScheme(string color) : base(color) {}

    override public DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeFruitSalad(sourceColorHct: seedHct, isDark: isDark, contrastLevel: contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider) => this;
}
