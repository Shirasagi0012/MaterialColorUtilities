using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia;

using DynamicColor;
using global::Avalonia;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using HCT;
using Scheme;

public class DynamicMaterialColorScheme : ResourceProvider
{

    public readonly static DirectProperty<DynamicMaterialColorScheme, Color> SeedColorProperty =
        AvaloniaProperty.RegisterDirect<DynamicMaterialColorScheme, Color>(
            name: nameof(SeedColor),
            getter: o => o.SeedColor,
            setter: (o, v) => o.SeedColor = v
        );

    public Color SeedColor
    {
        get;
        set
        {
            if (SetAndRaise(property: SeedColorProperty, field: ref field, value: value)) InvalidateSchemes();
        }
    }

    public readonly static DirectProperty<DynamicMaterialColorScheme, Variant> VariantProperty =
        AvaloniaProperty.RegisterDirect<DynamicMaterialColorScheme, Variant>(
            name: nameof(Variant),
            getter: o => o.Variant,
            setter: (o, v) => o.Variant = v
        );

    public Variant Variant
    {
        get;
        set
        {
            if (SetAndRaise(property: VariantProperty, field: ref field, value: value)) InvalidateSchemes();
        }
    } = Variant.TonalSpot;

    public readonly static DirectProperty<DynamicMaterialColorScheme, double> ContrastLevelProperty =
        AvaloniaProperty.RegisterDirect<DynamicMaterialColorScheme, double>(
            name: nameof(ContrastLevel),
            getter: o => o.ContrastLevel,
            setter: (o, v) => o.ContrastLevel = v
        );

    public double ContrastLevel
    {
        get;
        set
        {
            if (SetAndRaise(property: ContrastLevelProperty, field: ref field, value: value)) InvalidateSchemes();
        }
    }

    private int _schemesDirtyFlag;
    private int _updatePending;

    private void InvalidateSchemes()
    {
        Interlocked.Exchange(location1: ref _schemesDirtyFlag, value: 1);

        if (Interlocked.Exchange(location1: ref _updatePending, value: 1) == 1) return;

        Interlocked.Exchange(location1: ref _updatePending, value: 0);

        if (Interlocked.Exchange(location1: ref _schemesDirtyFlag, value: 0) == 0) return;
        UpdateSchemes();
    }

    private void UpdateSchemes()
    {
        var hct = Hct.FromAvaloniaColor(SeedColor);
        _lightColorScheme = GenerateScheme(false);
        _darkColorScheme = GenerateScheme(true);
        RaiseResourcesChanged();

        return;

        DynamicScheme GenerateScheme(bool isDark) => Variant switch
        {
            Variant.TonalSpot => new SchemeTonalSpot(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.Content => new SchemeContent(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.Vibrant => new SchemeVibrant(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.Expressive => new SchemeExpressive(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.Fidelity => new SchemeFidelity(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.Monochrome => new SchemeMonochrome(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.Neutral => new SchemeNeutral(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.Rainbow => new SchemeRainbow(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            Variant.FruitSalad => new SchemeFruitSalad(sourceColorHct: hct, isDark: isDark, contrastLevel: ContrastLevel),
            _ => throw new NotImplementedException(),
        };
    }

    private DynamicScheme? _lightColorScheme;
    private DynamicScheme? _darkColorScheme;

    override public bool TryGetResource(
        object key,
        ThemeVariant? theme,
        out object? value
    )
    {
        value = null;
        theme ??= ThemeVariant.Light;

        if (key is not string k) return false;
        value = (k, theme) switch
        {
            ("md.sys.color.background", var _)           => GetColor(color: MaterialDynamicColors.Background, variant: theme),
            ("md.sys.color.on-background", var _)         => GetColor(color: MaterialDynamicColors.OnBackground, variant: theme),
            ("md.sys.color.surface", var _)        => GetColor(color: MaterialDynamicColors.Surface, variant: theme),
            ("md.sys.color.surface-dim", var _)    => GetColor(color: MaterialDynamicColors.SurfaceDim, variant: theme),
            ("md.sys.color.surface-bright", var _) => GetColor(color: MaterialDynamicColors.SurfaceBright, variant: theme),
            ("md.sys.color.surface-container-lowest", var _) => GetColor(color: MaterialDynamicColors.SurfaceContainerLowest, variant: theme),
            ("md.sys.color.surface-container-low", var _)  => GetColor(color: MaterialDynamicColors.SurfaceContainerLow, variant: theme),
            ("md.sys.color.surface-container", var _)      => GetColor(color: MaterialDynamicColors.SurfaceContainer, variant: theme),
            ("md.sys.color.surface-container-high", var _) => GetColor(color: MaterialDynamicColors.SurfaceContainerHigh, variant: theme),
            ("md.sys.color.surface-container-highest", var _) => GetColor(color: MaterialDynamicColors.SurfaceContainerHighest, variant: theme),
            ("md.sys.color.on-surface", var _)               => GetColor(color: MaterialDynamicColors.OnSurface, variant: theme),
            ("md.sys.color.surface-variant", var _)          => GetColor(color: MaterialDynamicColors.SurfaceVariant, variant: theme),
            ("md.sys.color.on-surface-variant", var _)       => GetColor(color: MaterialDynamicColors.OnSurfaceVariant, variant: theme),
            ("md.sys.color.inverse-surface", var _)          => GetColor(color: MaterialDynamicColors.InverseSurface, variant: theme),
            ("md.sys.color.inverse-on-surface", var _)       => GetColor(color: MaterialDynamicColors.InverseOnSurface, variant: theme),
            ("md.sys.color.outline", var _)                  => GetColor(color: MaterialDynamicColors.Outline, variant: theme),
            ("md.sys.color.outline-variant", var _)          => GetColor(color: MaterialDynamicColors.OutlineVariant, variant: theme),
            ("md.sys.color.shadow", var _)                   => GetColor(color: MaterialDynamicColors.Shadow, variant: theme),
            ("md.sys.color.scrim", var _)                    => GetColor(color: MaterialDynamicColors.Scrim, variant: theme),
            ("md.sys.color.surface-tint", var _)             => GetColor(color: MaterialDynamicColors.SurfaceTint, variant: theme),
            ("md.sys.color.primary", var _)                  => GetColor(color: MaterialDynamicColors.Primary, variant: theme),
            ("md.sys.color.on-primary", var _)               => GetColor(color: MaterialDynamicColors.OnPrimary, variant: theme),
            ("md.sys.color.primary-container", var _)        => GetColor(color: MaterialDynamicColors.PrimaryContainer, variant: theme),
            ("md.sys.color.on-primary-container", var _)     => GetColor(color: MaterialDynamicColors.OnPrimaryContainer, variant: theme),
            ("md.sys.color.inverse-primary", var _)          => GetColor(color: MaterialDynamicColors.InversePrimary, variant: theme),
            ("md.sys.color.secondary", var _)                => GetColor(color: MaterialDynamicColors.Secondary, variant: theme),
            ("md.sys.color.on-secondary", var _)             => GetColor(color: MaterialDynamicColors.OnSecondary, variant: theme),
            ("md.sys.color.secondary-container", var _)      => GetColor(color: MaterialDynamicColors.SecondaryContainer, variant: theme),
            ("md.sys.color.on-secondary-container", var _)   => GetColor(color: MaterialDynamicColors.OnSecondaryContainer, variant: theme),
            ("md.sys.color.tertiary", var _)                 => GetColor(color: MaterialDynamicColors.Tertiary, variant: theme),
            ("md.sys.color.on-tertiary", var _)              => GetColor(color: MaterialDynamicColors.OnTertiary, variant: theme),
            ("md.sys.color.tertiary-container", var _)       => GetColor(color: MaterialDynamicColors.TertiaryContainer, variant: theme),
            ("md.sys.color.on-tertiary-container", var _)    => GetColor(color: MaterialDynamicColors.OnTertiaryContainer, variant: theme),
            ("md.sys.color.error", var _)                    => GetColor(color: MaterialDynamicColors.Error, variant: theme),
            ("md.sys.color.on-error", var _)                 => GetColor(color: MaterialDynamicColors.OnError, variant: theme),
            ("md.sys.color.error-container", var _)          => GetColor(color: MaterialDynamicColors.ErrorContainer, variant: theme),
            ("md.sys.color.on-error-container", var _)       => GetColor(color: MaterialDynamicColors.OnErrorContainer, variant: theme),
            ("md.sys.color.primary-fixed", var _)            => GetColor(color: MaterialDynamicColors.PrimaryFixed, variant: theme),
            ("md.sys.color.primary-fixed-dim", var _)        => GetColor(color: MaterialDynamicColors.PrimaryFixedDim, variant: theme),
            ("md.sys.color.on-primary-fixed", var _)         => GetColor(color: MaterialDynamicColors.OnPrimaryFixed, variant: theme),
            ("md.sys.color.on-primary-fixed-variant", var _) => GetColor(color: MaterialDynamicColors.OnPrimaryFixedVariant, variant: theme),
            ("md.sys.color.secondary-fixed", var _)          => GetColor(color: MaterialDynamicColors.SecondaryFixed, variant: theme),
            ("md.sys.color.secondary-fixed-dim", var _)      => GetColor(color: MaterialDynamicColors.SecondaryFixedDim, variant: theme),
            ("md.sys.color.on-secondary-fixed", var _)       => GetColor(color: MaterialDynamicColors.OnSecondaryFixed, variant: theme),
            ("md.sys.color.on-secondary-fixed-variant", var _) => GetColor(color: MaterialDynamicColors.OnSecondaryFixedVariant, variant: theme),
            ("md.sys.color.tertiary-fixed", var _)     => GetColor(color: MaterialDynamicColors.TertiaryFixed, variant: theme),
            ("md.sys.color.tertiary-fixed-dim", var _) => GetColor(color: MaterialDynamicColors.TertiaryFixedDim, variant: theme),
            ("md.sys.color.on-tertiary-fixed", var _)  => GetColor(color: MaterialDynamicColors.OnTertiaryFixed, variant: theme),
            ("md.sys.color.on-tertiary-fixed-variant", var _) => GetColor(color: MaterialDynamicColors.OnTertiaryFixedVariant, variant: theme),
            var _ => null,
        };

        if (value is {})
            return true;
        else
            return false;
    }

    private Color? GetColor(DynamicColor color, ThemeVariant variant) => variant == ThemeVariant.Dark
        ? _darkColorScheme?.GetHct(color)
           .ToAvaloniaColor()
        : _lightColorScheme?.GetHct(color)
           .ToAvaloniaColor();

    override public bool HasResources => _lightColorScheme is {} && _darkColorScheme is {};
}
