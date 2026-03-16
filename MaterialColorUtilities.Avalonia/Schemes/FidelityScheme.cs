namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class FidelityScheme : ColorScheme
{
    public FidelityScheme() : base()
    {
    }

    public FidelityScheme(BindingBase binding) : base(binding)
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
        var specVersion = ResolveSpecVersion();
        var platform = ResolvePlatform();
        var isDark = IsDark(theme);

        return new SchemeFidelity(seedHct, isDark, contrast, specVersion, platform);
    }

    public ColorScheme ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
