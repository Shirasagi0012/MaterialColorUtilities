namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using Scheme;

public class ContentScheme : SchemeProviderBase
{

    public ContentScheme() : base() {}

    public ContentScheme(IBinding binding) : base(binding) {}

    public ContentScheme(Color color) : base(color) {}

    public ContentScheme(string color) : base(color) {}

    override public DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var seedHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var isDark = theme == ThemeVariant.Dark;

        return new SchemeContent(sourceColorHct: seedHct, isDark: isDark, contrastLevel: contrast);
    }

    public ISchemeProvider ProvideTypedValue(IServiceProvider serviceProvider) => this;
}
