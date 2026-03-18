using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using DesignTokens;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.DynamicColors;
using ArgbColor = MaterialColorUtilities.Utils.ArgbColor;

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
        value = ResolveRef(key.Value.Palette, key.Value.Tone);

        return true;
    }

    bool ITokenResolver<Color, SysColorTokenKey>.TryResolve(
        TokenKey<Color, SysColorTokenKey> key,
        ThemeVariant themeVariant,
        AvaloniaObject? hostObject,
        out Color value
    )
    {
        value = ResolveSys(key.Value.Token, themeVariant);

        return true;
    }

    internal Color ResolveSys(SysColorToken token, ThemeVariant themeVariant)
    {
        var dynamicScheme = ColorScheme.IsDark(themeVariant) ? _darkScheme : _lightScheme;
        return ResolveSysArgb(dynamicScheme, token).ToAvaloniaColor();
    }

    internal Color ResolveRef(RefPaletteToken palette, byte tone)
    {
        return ResolveRefArgb(_lightScheme, palette, tone).ToAvaloniaColor();
    }

    private static ArgbColor ResolveSysArgb(DynamicScheme scheme, SysColorToken token)
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
            SysColorToken.Custom => throw new ArgumentOutOfRangeException(nameof(token)),
            SysColorToken.OnCustom => throw new ArgumentOutOfRangeException(nameof(token)),
            SysColorToken.CustomContainer => throw new ArgumentOutOfRangeException(nameof(token)),
            SysColorToken.OnCustomContainer => throw new ArgumentOutOfRangeException(nameof(token)),
            _ => throw new ArgumentOutOfRangeException(nameof(token))
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
            RefPaletteToken.Custom => throw new ArgumentOutOfRangeException(nameof(palette)),
            _ => throw new ArgumentOutOfRangeException(nameof(palette))
        };
    }
}
