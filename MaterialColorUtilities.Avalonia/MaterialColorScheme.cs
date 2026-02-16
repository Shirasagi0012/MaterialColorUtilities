using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using ArgbColor = MaterialColorUtilities.Utils.ArgbColor;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColorScheme : AvaloniaObject
{
    public static readonly DirectProperty<MaterialColorScheme, ISchemeProvider?> SchemeProperty =
        AvaloniaProperty.RegisterDirect<MaterialColorScheme, ISchemeProvider?>(
            nameof(Scheme),
            scheme => scheme.Scheme,
            (scheme, provider) => scheme.Scheme = provider
        );

    public static readonly DirectProperty<MaterialColorScheme, DynamicScheme?> LightSchemeProperty =
        AvaloniaProperty.RegisterDirect<MaterialColorScheme, DynamicScheme?>(
            nameof(LightScheme),
            scheme => scheme.LightScheme
        );

    public static readonly DirectProperty<MaterialColorScheme, DynamicScheme?> DarkSchemeProperty =
        AvaloniaProperty.RegisterDirect<MaterialColorScheme, DynamicScheme?>(
            nameof(DarkScheme),
            scheme => scheme.DarkScheme
        );

    public static readonly DirectProperty<MaterialColorScheme, int> RevisionProperty =
        AvaloniaProperty.RegisterDirect<MaterialColorScheme, int>(
            nameof(Revision),
            scheme => scheme.Revision
        );

    private DynamicScheme? _lightScheme;
    private DynamicScheme? _darkScheme;
    private int _revision;
    private Dictionary<string, TonalPalette> _customPalettes = new(StringComparer.OrdinalIgnoreCase);

    public MaterialColorScheme()
    {
        CustomColors.CollectionChanged += OnCustomColorsChanged;
        Refresh();
    }

    public MaterialColorScheme(ISchemeProvider scheme)
    {
        CustomColors.CollectionChanged += OnCustomColorsChanged;
        Scheme = scheme;
    }

    public event EventHandler? Changed;

    public DynamicScheme? LightScheme
    {
        get => _lightScheme;
        private set => SetAndRaise(LightSchemeProperty, ref _lightScheme, value);
    }

    public DynamicScheme? DarkScheme
    {
        get => _darkScheme;
        private set => SetAndRaise(DarkSchemeProperty, ref _darkScheme, value);
    }

    public int Revision
    {
        get => _revision;
        private set => SetAndRaise(RevisionProperty, ref _revision, value);
    }

    [ConstructorArgument("scheme")]
    public ISchemeProvider? Scheme
    {
        get;
        set
        {
            if (field is not null)
                field.SchemeChanged -= OnSchemeProviderChanged;

            SetAndRaise(SchemeProperty, ref field, value);

            if (value is not null)
                value.SchemeChanged += OnSchemeProviderChanged;

            Refresh();
        }
    }

    [Content]
    public AvaloniaList<MaterialCustomColor> CustomColors { get; } = [];

    public Color? Resolve(SysColorToken token, ThemeVariant themeVariant)
    {
        var scheme = ResolveDynamicScheme(themeVariant);
        if (scheme is null)
            return null;

        return ResolveSysArgb(scheme, token)?.ToAvaloniaColor();
    }

    public Color? Resolve(RefPaletteToken palette, byte tone, ThemeVariant themeVariant)
    {
        if (tone > 100)
            throw new ArgumentOutOfRangeException(nameof(tone), "Tone must be in range 0..100.");

        var scheme = ResolveDynamicScheme(themeVariant);
        if (scheme is null)
            return null;

        return ResolveRefArgb(scheme, palette, tone).ToAvaloniaColor();
    }

    public Color? Resolve(RefPaletteToken palette, byte tone)
    {
        return Resolve(palette, tone, ThemeVariant.Default);
    }

    public Color? Resolve(string key, SysColorToken role, ThemeVariant themeVariant)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var normalized = key.Trim();
        if (!_customPalettes.TryGetValue(normalized, out var palette))
            return null;

        var tone = GetCustomRoleTone(role, IsDark(themeVariant));
        return palette.Get(tone).ToAvaloniaColor();
    }

    public MaterialColorScheme ProvideValue(IServiceProvider serviceProvider) => this;

    private static ArgbColor? ResolveSysArgb(DynamicScheme scheme, SysColorToken token) =>
        token switch
        {
            SysColorToken.Background => scheme.Background,
            SysColorToken.OnBackground => scheme.OnBackground,
            SysColorToken.Surface => scheme.Surface,
            SysColorToken.SurfaceDim => scheme.SurfaceDim,
            SysColorToken.SurfaceBright => scheme.SurfaceBright,
            SysColorToken.SurfaceContainerLowest => scheme.SurfaceContainerLowest,
            SysColorToken.SurfaceContainerLow => scheme.SurfaceContainerLow,
            SysColorToken.SurfaceContainer => scheme.SurfaceContainer,
            SysColorToken.SurfaceContainerHigh => scheme.SurfaceContainerHigh,
            SysColorToken.SurfaceContainerHighest => scheme.SurfaceContainerHighest,
            SysColorToken.OnSurface => scheme.OnSurface,
            SysColorToken.SurfaceVariant => scheme.SurfaceVariant,
            SysColorToken.OnSurfaceVariant => scheme.OnSurfaceVariant,
            SysColorToken.InverseSurface => scheme.InverseSurface,
            SysColorToken.InverseOnSurface => scheme.InverseOnSurface,
            SysColorToken.Outline => scheme.Outline,
            SysColorToken.OutlineVariant => scheme.OutlineVariant,
            SysColorToken.Shadow => scheme.Shadow,
            SysColorToken.Scrim => scheme.Scrim,
            SysColorToken.SurfaceTint => scheme.SurfaceTint,
            SysColorToken.Primary => scheme.Primary,
            SysColorToken.OnPrimary => scheme.OnPrimary,
            SysColorToken.PrimaryContainer => scheme.PrimaryContainer,
            SysColorToken.OnPrimaryContainer => scheme.OnPrimaryContainer,
            SysColorToken.InversePrimary => scheme.InversePrimary,
            SysColorToken.Secondary => scheme.Secondary,
            SysColorToken.OnSecondary => scheme.OnSecondary,
            SysColorToken.SecondaryContainer => scheme.SecondaryContainer,
            SysColorToken.OnSecondaryContainer => scheme.OnSecondaryContainer,
            SysColorToken.Tertiary => scheme.Tertiary,
            SysColorToken.OnTertiary => scheme.OnTertiary,
            SysColorToken.TertiaryContainer => scheme.TertiaryContainer,
            SysColorToken.OnTertiaryContainer => scheme.OnTertiaryContainer,
            SysColorToken.Error => scheme.Error,
            SysColorToken.OnError => scheme.OnError,
            SysColorToken.ErrorContainer => scheme.ErrorContainer,
            SysColorToken.OnErrorContainer => scheme.OnErrorContainer,
            SysColorToken.PrimaryFixed => scheme.PrimaryFixed,
            SysColorToken.PrimaryFixedDim => scheme.PrimaryFixedDim,
            SysColorToken.OnPrimaryFixed => scheme.OnPrimaryFixed,
            SysColorToken.OnPrimaryFixedVariant => scheme.OnPrimaryFixedVariant,
            SysColorToken.SecondaryFixed => scheme.SecondaryFixed,
            SysColorToken.SecondaryFixedDim => scheme.SecondaryFixedDim,
            SysColorToken.OnSecondaryFixed => scheme.OnSecondaryFixed,
            SysColorToken.OnSecondaryFixedVariant => scheme.OnSecondaryFixedVariant,
            SysColorToken.TertiaryFixed => scheme.TertiaryFixed,
            SysColorToken.TertiaryFixedDim => scheme.TertiaryFixedDim,
            SysColorToken.OnTertiaryFixed => scheme.OnTertiaryFixed,
            SysColorToken.OnTertiaryFixedVariant => scheme.OnTertiaryFixedVariant,
            _ => null
        };

    private static ArgbColor ResolveRefArgb(DynamicScheme scheme, RefPaletteToken palette, byte tone) =>
        palette switch
        {
            RefPaletteToken.Primary => scheme.PrimaryPalette.Get(tone),
            RefPaletteToken.Secondary => scheme.SecondaryPalette.Get(tone),
            RefPaletteToken.Tertiary => scheme.TertiaryPalette.Get(tone),
            RefPaletteToken.Neutral => scheme.NeutralPalette.Get(tone),
            RefPaletteToken.NeutralVariant => scheme.NeutralVariantPalette.Get(tone),
            RefPaletteToken.Error => scheme.ErrorPalette.Get(tone),
            _ => throw new ArgumentOutOfRangeException(nameof(palette))
        };

    private static int GetCustomRoleTone(SysColorToken role, bool isDark) =>
        (role, isDark) switch
        {
            (SysColorToken.Custom, false) => 40,
            (SysColorToken.OnCustom, false) => 100,
            (SysColorToken.CustomContainer, false) => 90,
            (SysColorToken.OnCustomContainer, false) => 10,
            (SysColorToken.Custom, true) => 80,
            (SysColorToken.OnCustom, true) => 20,
            (SysColorToken.CustomContainer, true) => 30,
            (SysColorToken.OnCustomContainer, true) => 90,
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };

    private DynamicScheme? ResolveDynamicScheme(ThemeVariant variant) =>
        IsDark(variant) ? DarkScheme ?? LightScheme : LightScheme ?? DarkScheme;

    private static bool IsDark(ThemeVariant variant)
    {
        if (variant == ThemeVariant.Dark)
            return true;

        if (variant == ThemeVariant.Light)
            return false;

        var inherited = variant.InheritVariant;
        while (inherited is not null)
        {
            if (inherited == ThemeVariant.Dark)
                return true;

            if (inherited == ThemeVariant.Light)
                return false;

            inherited = inherited.InheritVariant;
        }

        return false;
    }

    private void OnSchemeProviderChanged(object? sender, EventArgs e) => Refresh();

    private void OnCustomColorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var entry in e.OldItems.OfType<MaterialCustomColor>())
                entry.PropertyChanged -= OnCustomColorPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (var entry in e.NewItems.OfType<MaterialCustomColor>())
                entry.PropertyChanged += OnCustomColorPropertyChanged;
        }

        Refresh();
    }

    private void OnCustomColorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MaterialCustomColor.KeyProperty
            || e.Property == MaterialCustomColor.ColorProperty
            || e.Property == MaterialCustomColor.BlendProperty)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        var provider = Scheme;

        try
        {
            LightScheme = provider?.CreateScheme(ThemeVariant.Light);
            DarkScheme = provider?.CreateScheme(ThemeVariant.Dark);
        }
        catch
        {
            LightScheme = null;
            DarkScheme = null;
        }

        _customPalettes = BuildCustomPalettes();
        Revision = unchecked(Revision + 1);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private Dictionary<string, TonalPalette> BuildCustomPalettes()
    {
        var result = new Dictionary<string, TonalPalette>(StringComparer.OrdinalIgnoreCase);
        var sourceArgb = LightScheme?.SourceColorArgb ?? DarkScheme?.SourceColorArgb;

        foreach (var entry in CustomColors)
        {
            if (entry.Color is not { } color || string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var key = entry.Key.Trim();
            var colorArgb = ArgbExtensions.FromAvaloniaColor(color);
            var resolvedArgb =
                entry.Blend && sourceArgb is { } source
                    ? MaterialColorUtilities.Blend.Blend.Harmonize(colorArgb, source)
                    : colorArgb;

            var hct = Hct.From(resolvedArgb);
            result[key] = new TonalPalette(hct);
        }

        return result;
    }
}

public class MaterialColorSchemeExtension : MaterialColorScheme
{
    public MaterialColorSchemeExtension()
    {
    }

    public MaterialColorSchemeExtension(ISchemeProvider scheme) : base(scheme)
    {
    }

    public MaterialColorScheme ProvideTypedValue(IServiceProvider serviceProvider) => this;
}