using System;
using Avalonia;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using HCT;
using Scheme;
using Utils;

public class CmfScheme : ColorScheme
{
    public static readonly StyledProperty<Color?> SecondaryColorProperty =
        AvaloniaProperty.Register<CmfScheme, Color?>(nameof(SecondaryColor));

    public Color? SecondaryColor
    {
        get => GetValue(SecondaryColorProperty);
        set => SetValue(SecondaryColorProperty, value);
    }

    static CmfScheme()
    {
        SecondaryColorProperty.Changed.AddClassHandler<CmfScheme>((x, _) => x.OnSchemeChanged());
    }

    public CmfScheme() : base()
    {
    }

    public CmfScheme(BindingBase binding) : base(binding)
    {
    }

    public CmfScheme(Color color) : base(color)
    {
    }

    public CmfScheme(string color) : base(color)
    {
    }

    public override DynamicScheme CreateScheme(ThemeVariant theme)
    {
        var specVersion = ResolveSpecVersion();
        if (specVersion != ColorSpec.SpecVersion.Spec2026)
            throw new InvalidOperationException("CmfScheme requires SpecVersion to be Spec2026.");

        var sourceColorHct = ResolveSeedHct();
        var contrast = ResolveContrast();
        var platform = ResolvePlatform();
        var isDark = IsDark(theme);

        var sourceColorHctList = SecondaryColor is { } secondaryColor
            ? new[] { sourceColorHct, Hct.From(ArgbColor.FromAvaloniaColor(secondaryColor)) }
            : new[] { sourceColorHct };

        return new SchemeCmf(sourceColorHctList, isDark, contrast, specVersion, platform);
    }

    public ColorScheme ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
