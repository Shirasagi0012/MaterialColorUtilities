using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Avalonia.Tokens;

/// <summary>
/// An immutable snapshot of a <see cref="ColorScheme" />: both theme variants generated once,
/// plus the tonal palettes of any named custom colors.
/// </summary>
/// <remarks>
/// Consumers only ever see one of these, never the mutable <see cref="ColorScheme" /> that
/// produced it. A snapshot is replaced wholesale when the scheme changes, so a stale cache is not
/// representable. Resolved colors and brushes are memoized per (token, variant) because color
/// resolution is the hot path for every control.
/// </remarks>
public sealed class ResolvedColorScheme
{
    private static readonly int SysTokenCount = Enum.GetValues<SysColorToken>().Length;

    private readonly DynamicScheme _light;
    private readonly DynamicScheme _dark;
    private readonly IReadOnlyDictionary<string, TonalPalette> _customPalettes;
    private readonly Color?[] _colorCache;
    private readonly IImmutableBrush?[] _brushCache;

    public ResolvedColorScheme(ColorScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);

        _light = scheme.CreateScheme(ThemeVariant.Light);
        _dark = scheme.CreateScheme(ThemeVariant.Dark);
        _customPalettes = scheme.CreateCustomPalettes();
        _colorCache = new Color?[SysTokenCount * 2];
        _brushCache = new IImmutableBrush?[SysTokenCount * 2];
    }

    /// <summary>The generated scheme for the requested variant.</summary>
    public DynamicScheme GetDynamicScheme(bool isDark)
    {
        return isDark ? _dark : _light;
    }

    /// <summary>
    /// Resolves a system color role. Returns <see langword="false" /> when the token names a
    /// custom color and no <see cref="CustomColor" /> with that key was declared — callers then
    /// fall back rather than throwing, matching the binding's fallback semantics.
    /// </summary>
    public bool TryGetColor(SysColorToken token, bool isDark, string? customKey, out Color color)
    {
        if (IsCustom(token))
            return TryGetCustomColor(token, isDark, customKey, out color);

        var slot = CacheSlot(token, isDark);
        if (_colorCache[slot] is { } cached)
        {
            color = cached;
            return true;
        }

        color = Resolve(token, isDark).ToAvaloniaColor();
        _colorCache[slot] = color;
        return true;
    }

    /// <summary>
    /// Resolves a system color role, or <see cref="Colors.Transparent" /> when the token names an
    /// undeclared custom color.
    /// </summary>
    public Color GetColor(SysColorToken token, bool isDark, string? customKey = null)
    {
        return TryGetColor(token, isDark, customKey, out var color) ? color : Colors.Transparent;
    }

    /// <inheritdoc cref="GetColor(SysColorToken, bool, string?)" />
    public Color GetColor(SysColorToken token, ThemeVariant? variant, string? customKey = null)
    {
        return GetColor(token, ColorScheme.IsDark(variant), customKey);
    }

    /// <summary>
    /// Resolves a system color role to an immutable brush. Non-custom roles are memoized, so
    /// repeated resolution of the same role at the same variant returns the same instance.
    /// </summary>
    public bool TryGetBrush(SysColorToken token, bool isDark, string? customKey, out IImmutableBrush brush)
    {
        if (IsCustom(token))
        {
            if (!TryGetCustomColor(token, isDark, customKey, out var customColor))
            {
                brush = null!;
                return false;
            }

            brush = new ImmutableSolidColorBrush(customColor);
            return true;
        }

        var slot = CacheSlot(token, isDark);
        if (_brushCache[slot] is { } cached)
        {
            brush = cached;
            return true;
        }

        TryGetColor(token, isDark, customKey, out var color);
        brush = new ImmutableSolidColorBrush(color);
        _brushCache[slot] = brush;
        return true;
    }

    /// <inheritdoc cref="TryGetBrush" />
    public IImmutableBrush GetBrush(SysColorToken token, bool isDark, string? customKey = null)
    {
        return TryGetBrush(token, isDark, customKey, out var brush)
            ? brush
            : new ImmutableSolidColorBrush(Colors.Transparent);
    }

    /// <summary>
    /// Resolves a tone from one of the reference tonal palettes. Returns <see langword="false" />
    /// for <see cref="RefPaletteToken.Custom" /> with an undeclared key.
    /// </summary>
    public bool TryGetPaletteColor(RefPaletteToken palette, byte tone, string? customKey, out Color color)
    {
        if (palette == RefPaletteToken.Custom)
        {
            if (!TryGetCustomPalette(customKey, out var customPalette))
            {
                color = default;
                return false;
            }

            color = customPalette.Get(tone).ToAvaloniaColor();
            return true;
        }

        // The reference palettes are variant-independent: both generated schemes share the seed.
        color = (palette switch
        {
            RefPaletteToken.Primary => _light.PrimaryPalette,
            RefPaletteToken.Secondary => _light.SecondaryPalette,
            RefPaletteToken.Tertiary => _light.TertiaryPalette,
            RefPaletteToken.Neutral => _light.NeutralPalette,
            RefPaletteToken.NeutralVariant => _light.NeutralVariantPalette,
            RefPaletteToken.Error => _light.ErrorPalette,
            _ => throw new ArgumentOutOfRangeException(nameof(palette), palette, null)
        }).Get(tone).ToAvaloniaColor();

        return true;
    }

    /// <inheritdoc cref="TryGetPaletteColor" />
    public Color GetPaletteColor(RefPaletteToken palette, byte tone, string? customKey = null)
    {
        return TryGetPaletteColor(palette, tone, customKey, out var color) ? color : Colors.Transparent;
    }

    private static bool IsCustom(SysColorToken token)
    {
        return token is SysColorToken.Custom or SysColorToken.OnCustom or SysColorToken.CustomContainer
            or SysColorToken.OnCustomContainer;
    }

    private static int CacheSlot(SysColorToken token, bool isDark)
    {
        return (int)token * 2 + (isDark ? 1 : 0);
    }

    private bool TryGetCustomColor(SysColorToken token, bool isDark, string? customKey, out Color color)
    {
        if (!TryGetCustomPalette(customKey, out var palette))
        {
            color = default;
            return false;
        }

        // Tones per the Material 3 custom-color spec; unlike the generated roles these are fixed
        // and do not respond to the contrast level.
        var tone = (token, isDark) switch
        {
            (SysColorToken.Custom, false) => 40,
            (SysColorToken.OnCustom, false) => 100,
            (SysColorToken.CustomContainer, false) => 90,
            (SysColorToken.OnCustomContainer, false) => 10,
            (SysColorToken.Custom, true) => 80,
            (SysColorToken.OnCustom, true) => 20,
            (SysColorToken.CustomContainer, true) => 30,
            (SysColorToken.OnCustomContainer, true) => 90,
            _ => throw new ArgumentOutOfRangeException(nameof(token), token, null)
        };

        color = palette.Get(tone).ToAvaloniaColor();
        return true;
    }

    private bool TryGetCustomPalette(string? customKey, out TonalPalette palette)
    {
        if (customKey is { Length: > 0 })
            return _customPalettes.TryGetValue(customKey, out palette!);

        palette = null!;
        return false;
    }

    private ArgbColor Resolve(SysColorToken token, bool isDark)
    {
        var scheme = isDark ? _dark : _light;

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
            _ => throw new ArgumentOutOfRangeException(nameof(token), token, null)
        };
    }
}
