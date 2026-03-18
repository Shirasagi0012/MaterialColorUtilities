using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using DesignTokens;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.DynamicColors;

namespace MaterialColorUtilities.Avalonia.Tokens;

internal sealed class MaterialColorScheme(ColorScheme scheme)
    : ITokenResolver<Color, RefPaletteTokenKey>, ITokenResolver<Color, SysColorTokenKey>
{
    private readonly DynamicScheme _lightScheme = scheme.CreateScheme(ThemeVariant.Light);
    private readonly DynamicScheme _darkScheme = scheme.CreateScheme(ThemeVariant.Dark);

    bool ITokenResolver<Color, RefPaletteTokenKey>.TryResolve(
        TokenKey<Color, RefPaletteTokenKey> key,
        ThemeVariant themeVariant,
        AvaloniaObject? hostObject,
        out Color value
    )
    {
        value = (key.Value.Palette switch
        {
            RefPaletteToken.Primary => _lightScheme.PrimaryPalette.Get(key.Value.Tone),
            RefPaletteToken.Secondary => _lightScheme.SecondaryPalette.Get(key.Value.Tone),
            RefPaletteToken.Tertiary => _lightScheme.TertiaryPalette.Get(key.Value.Tone),
            RefPaletteToken.Neutral => _lightScheme.NeutralPalette.Get(key.Value.Tone),
            RefPaletteToken.NeutralVariant => _lightScheme.NeutralVariantPalette.Get(key.Value.Tone),
            RefPaletteToken.Error => _lightScheme.ErrorPalette.Get(key.Value.Tone),
            RefPaletteToken.Custom => throw new ArgumentOutOfRangeException(nameof(key)),
            _ => throw new ArgumentOutOfRangeException(nameof(key))
        }).ToAvaloniaColor();

        return true;
    }

    bool ITokenResolver<Color, SysColorTokenKey>.TryResolve(
        TokenKey<Color, SysColorTokenKey> key,
        ThemeVariant themeVariant,
        AvaloniaObject? hostObject,
        out Color value
    )
    {
        var dynamicScheme = ColorScheme.IsDark(themeVariant) ? _darkScheme : _lightScheme;
        value = (key.Value.Token switch
        {
            SysColorToken.Background => dynamicScheme.Background,
            SysColorToken.OnBackground => dynamicScheme.OnBackground,
            SysColorToken.Surface => dynamicScheme.Surface,
            SysColorToken.SurfaceDim => dynamicScheme.SurfaceDim,
            SysColorToken.SurfaceBright => dynamicScheme.SurfaceBright,
            SysColorToken.SurfaceContainerLowest => dynamicScheme.SurfaceContainerLowest,
            SysColorToken.SurfaceContainerLow => dynamicScheme.SurfaceContainerLow,
            SysColorToken.SurfaceContainer => dynamicScheme.SurfaceContainer,
            SysColorToken.SurfaceContainerHigh => dynamicScheme.SurfaceContainerHigh,
            SysColorToken.SurfaceContainerHighest => dynamicScheme.SurfaceContainerHighest,
            SysColorToken.OnSurface => dynamicScheme.OnSurface,
            SysColorToken.SurfaceVariant => dynamicScheme.SurfaceVariant,
            SysColorToken.OnSurfaceVariant => dynamicScheme.OnSurfaceVariant,
            SysColorToken.InverseSurface => dynamicScheme.InverseSurface,
            SysColorToken.InverseOnSurface => dynamicScheme.InverseOnSurface,
            SysColorToken.Outline => dynamicScheme.Outline,
            SysColorToken.OutlineVariant => dynamicScheme.OutlineVariant,
            SysColorToken.Shadow => dynamicScheme.Shadow,
            SysColorToken.Scrim => dynamicScheme.Scrim,
            SysColorToken.SurfaceTint => dynamicScheme.SurfaceTint,
            SysColorToken.Primary => dynamicScheme.Primary,
            SysColorToken.OnPrimary => dynamicScheme.OnPrimary,
            SysColorToken.PrimaryContainer => dynamicScheme.PrimaryContainer,
            SysColorToken.OnPrimaryContainer => dynamicScheme.OnPrimaryContainer,
            SysColorToken.InversePrimary => dynamicScheme.InversePrimary,
            SysColorToken.Secondary => dynamicScheme.Secondary,
            SysColorToken.OnSecondary => dynamicScheme.OnSecondary,
            SysColorToken.SecondaryContainer => dynamicScheme.SecondaryContainer,
            SysColorToken.OnSecondaryContainer => dynamicScheme.OnSecondaryContainer,
            SysColorToken.Tertiary => dynamicScheme.Tertiary,
            SysColorToken.OnTertiary => dynamicScheme.OnTertiary,
            SysColorToken.TertiaryContainer => dynamicScheme.TertiaryContainer,
            SysColorToken.OnTertiaryContainer => dynamicScheme.OnTertiaryContainer,
            SysColorToken.Error => dynamicScheme.Error,
            SysColorToken.OnError => dynamicScheme.OnError,
            SysColorToken.ErrorContainer => dynamicScheme.ErrorContainer,
            SysColorToken.OnErrorContainer => dynamicScheme.OnErrorContainer,
            SysColorToken.PrimaryFixed => dynamicScheme.PrimaryFixed,
            SysColorToken.PrimaryFixedDim => dynamicScheme.PrimaryFixedDim,
            SysColorToken.OnPrimaryFixed => dynamicScheme.OnPrimaryFixed,
            SysColorToken.OnPrimaryFixedVariant => dynamicScheme.OnPrimaryFixedVariant,
            SysColorToken.SecondaryFixed => dynamicScheme.SecondaryFixed,
            SysColorToken.SecondaryFixedDim => dynamicScheme.SecondaryFixedDim,
            SysColorToken.OnSecondaryFixed => dynamicScheme.OnSecondaryFixed,
            SysColorToken.OnSecondaryFixedVariant => dynamicScheme.OnSecondaryFixedVariant,
            SysColorToken.TertiaryFixed => dynamicScheme.TertiaryFixed,
            SysColorToken.TertiaryFixedDim => dynamicScheme.TertiaryFixedDim,
            SysColorToken.OnTertiaryFixed => dynamicScheme.OnTertiaryFixed,
            SysColorToken.OnTertiaryFixedVariant => dynamicScheme.OnTertiaryFixedVariant,
            SysColorToken.Custom => throw new ArgumentOutOfRangeException(nameof(key)),
            SysColorToken.OnCustom => throw new ArgumentOutOfRangeException(nameof(key)),
            SysColorToken.CustomContainer => throw new ArgumentOutOfRangeException(nameof(key)),
            SysColorToken.OnCustomContainer => throw new ArgumentOutOfRangeException(nameof(key)),
            _ => throw new ArgumentOutOfRangeException(nameof(key))
        }).ToAvaloniaColor();

        return true;
    }
}
