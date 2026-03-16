namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class VibrantScheme : ColorScheme
{
    public VibrantScheme() : base()
    {
    }

    public VibrantScheme(BindingBase binding) : base(binding)
    {
    }

    public VibrantScheme(Color color) : base(color)
    {
    }

    public VibrantScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var specVersion = ResolveSpecVersion();
        var platform = ResolvePlatform();
        var isDark = IsDark(theme);

        return new SchemeVibrant(seedHct, isDark, contrast, specVersion, platform);
    }

    public ColorScheme ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
