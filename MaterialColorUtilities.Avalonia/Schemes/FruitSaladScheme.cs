namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class FruitSaladScheme : ColorScheme
{
    public FruitSaladScheme() : base()
    {
    }

    public FruitSaladScheme(BindingBase binding) : base(binding)
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
        var specVersion = ResolveSpecVersion();
        var platform = ResolvePlatform();
        var isDark = IsDark(theme);

        return new SchemeFruitSalad(seedHct, isDark, contrast, specVersion, platform);
    }

    public ColorScheme ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
