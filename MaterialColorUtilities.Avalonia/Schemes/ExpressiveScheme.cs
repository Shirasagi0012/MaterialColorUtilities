namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class ExpressiveScheme : SchemeProviderBase
{

    public ExpressiveScheme() : base() {}

    public ExpressiveScheme(IBinding binding) : base(binding) {}

    public ExpressiveScheme(Color color) : base(color) {}

    public ExpressiveScheme(string color) : base(color) {}

    override public DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeExpressive(sourceColorHct: seedHct, isDark: isDark, contrastLevel: contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider) => this;
}
