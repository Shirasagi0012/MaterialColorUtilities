using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
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

    internal MaterialColorSchemeInternal Internal { get; } = new();

    public MaterialColorScheme()
    {
    }

    public MaterialColorScheme(ISchemeProvider scheme)
    {
        Scheme = scheme;
    }

    public MaterialColorScheme(IBinding scheme)
    {
        this[!SchemeProperty] = scheme;
    }

    [ConstructorArgument("scheme")]
    public ISchemeProvider? Scheme
    {
        get;
        set
        {
            if (value == field) return;
            
            if (field is { })
                field.SchemeChanged -= OnSchemeProviderChanged;

            SetAndRaise(SchemeProperty, ref field, value);

            if (value is { })
                value.SchemeChanged += OnSchemeProviderChanged;

            Internal.UpdateScheme(Scheme);
        }
    }

    private void OnSchemeProviderChanged(object? sender, EventArgs e)
    {
        Internal.UpdateScheme(Scheme);
    }

    internal class MaterialColorSchemeInternal : INotifyPropertyChanged
    {
        public MaterialColorSchemeInternal()
        {
            CustomColors.ResetBehavior = ResetBehavior.Remove;
            CustomColors.CollectionChanged += OnCustomColorsChanged;
        }
        
        private Dictionary<string, TonalPalette> _customPalettes = new(StringComparer.OrdinalIgnoreCase);
        
        [Content] public AvaloniaList<MaterialCustomColor> CustomColors { get; } = [];
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public DynamicScheme? LightScheme { get; private set; }

        public DynamicScheme? DarkScheme { get; private set; }
        
        public void UpdateScheme(ISchemeProvider? provider)
        {
            LightScheme = provider?.CreateScheme(ThemeVariant.Light);
            DarkScheme = provider?.CreateScheme(ThemeVariant.Dark);

            BuildCustomPalettes(CustomColors);
        }

        private static bool IsDark(ThemeVariant variant)
        {
            if (variant == ThemeVariant.Dark)
                return true;

            if (variant == ThemeVariant.Light)
                return false;

            var inherited = variant.InheritVariant;
            while (inherited is { })
            {
                if (inherited == ThemeVariant.Dark)
                    return true;

                if (inherited == ThemeVariant.Light)
                    return false;

                inherited = inherited.InheritVariant;
            }

            return false;
        }

        private void BuildCustomPalettes(IEnumerable<MaterialCustomColor> customColors)
        {
            var result = new Dictionary<string, TonalPalette>(StringComparer.OrdinalIgnoreCase);
            var sourceArgb = LightScheme?.SourceColorArgb ?? DarkScheme?.SourceColorArgb;

            foreach (var entry in customColors)
            {
                if (entry.Color is not { } color || String.IsNullOrWhiteSpace(entry.Key))
                    continue;

                var key = entry.Key.Trim();
                var colorArgb = ArgbColor.FromAvaloniaColor(color);
                var resolvedArgb =
                    entry.Blend && sourceArgb is { } source
                        ? Blend.Blend.Harmonize(colorArgb, source)
                        : colorArgb;

                var hct = Hct.From(resolvedArgb);
                result[key] = new TonalPalette(hct);
            }

            _customPalettes =  result;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        private DynamicScheme? ResolveDynamicScheme(ThemeVariant variant)
        {
            return IsDark(variant) ? DarkScheme ?? LightScheme : LightScheme ?? DarkScheme;
        }


        internal Color? ResolveSys(SysColorToken token, ThemeVariant themeVariant, string? customKey = null)
        {
            if (TokenHelper.IsCustom(token))
            {
                if (!TryGetCustomPalette(customKey, out var palette))
                    return null;

                var tone = GetCustomRoleTone(token, IsDark(themeVariant));
                return palette.Get(tone).ToAvaloniaColor();
            }

            var scheme = ResolveDynamicScheme(themeVariant);
            if (scheme is null)
                return null;

            return ResolveSysArgb(scheme, token)?.ToAvaloniaColor();
        }

        internal Color? ResolveRef(RefPaletteToken palette, byte tone, string? customKey = null)
        {
            if (TokenHelper.IsCustom(palette))
            {
                if (!TryGetCustomPalette(customKey, out var customPalette))
                    return null;

                return customPalette.Get(tone).ToAvaloniaColor();
            }

            var scheme = ResolveDynamicScheme(ThemeVariant.Default);
            if (scheme is null)
                return null;

            return ResolveRefArgb(scheme, palette, tone).ToAvaloniaColor();
        }

        private bool TryGetCustomPalette(string? customKey, [NotNullWhen(true)] out TonalPalette? palette)
        {
            palette = null;
            if (String.IsNullOrWhiteSpace(customKey))
                return false;

            var normalized = customKey.Trim();
            return _customPalettes.TryGetValue(normalized, out palette);
        }

        private static ArgbColor? ResolveSysArgb(DynamicScheme scheme, SysColorToken token)
        {
            return token switch
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
        }

        private static ArgbColor ResolveRefArgb(DynamicScheme scheme, RefPaletteToken palette, byte tone)
        {
            return palette switch
            {
                RefPaletteToken.Primary => scheme.PrimaryPalette.Get(tone),
                RefPaletteToken.Secondary => scheme.SecondaryPalette.Get(tone),
                RefPaletteToken.Tertiary => scheme.TertiaryPalette.Get(tone),
                RefPaletteToken.Neutral => scheme.NeutralPalette.Get(tone),
                RefPaletteToken.NeutralVariant => scheme.NeutralVariantPalette.Get(tone),
                RefPaletteToken.Error => scheme.ErrorPalette.Get(tone),
                _ => throw new ArgumentOutOfRangeException(nameof(palette))
            };
        }

        private static int GetCustomRoleTone(SysColorToken role, bool isDark)
        {
            return (role, isDark) switch
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
        }
        
        
        private void OnCustomColorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is { })
                foreach (var entry in e.OldItems.OfType<MaterialCustomColor>())
                    entry.PropertyChanged -= OnCustomColorPropertyChanged;

            if (e.NewItems is { })
                foreach (var entry in e.NewItems.OfType<MaterialCustomColor>())
                    entry.PropertyChanged += OnCustomColorPropertyChanged;

            BuildCustomPalettes(CustomColors);
        }

        private void OnCustomColorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == MaterialCustomColor.KeyProperty
                || e.Property == MaterialCustomColor.ColorProperty
                || e.Property == MaterialCustomColor.BlendProperty)
                BuildCustomPalettes(CustomColors);
        }
    }
}
