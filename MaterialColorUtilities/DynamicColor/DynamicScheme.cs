// Copyright 2022 Google LLC
//
//  This file is part of the material-color-utilities C# port by @Shirasagi0012
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.DynamicColor;

/// <summary>
/// Constructed by a set of values representing the current UI state (such as
/// whether or not its dark theme, what the theme style is, etc.), and
/// provides a set of TonalPalettes that can create colors that fit in
/// with the theme style. Used by DynamicColor to resolve into a color.
/// </summary>
public class DynamicScheme(
    Hct sourceColorHct,
    Variant variant,
    bool isDark,
    double contrastLevel,
    TonalPalette primaryPalette,
    TonalPalette secondaryPalette,
    TonalPalette tertiaryPalette,
    TonalPalette neutralPalette,
    TonalPalette neutralVariantPalette,
    TonalPalette? errorPalette = null
)
{
    /// <summary>
    /// The source color of the theme as an ARGB integer.
    /// </summary>
    public readonly int SourceColorArgb = sourceColorHct.Argb.Value;

    /// <summary>
    /// The source color of the theme in HCT.
    /// </summary>
    public readonly Hct SourceColorHct = sourceColorHct;

    /// <summary>
    /// The variant, or style, of the theme.
    /// </summary>
    public readonly Variant Variant = variant;

    /// <summary>
    /// Whether or not the scheme is in 'dark mode' or 'light mode'.
    /// </summary>
    public readonly bool IsDark = isDark;

    /// <summary>
    /// Value from -1 to 1. -1 represents minimum contrast, 0 represents
    /// standard (i.e. the design as spec'd), and 1 represents maximum contrast.
    /// </summary>
    public readonly double ContrastLevel = contrastLevel;

    /// <summary>
    /// Given a tone, produces a color. Hue and chroma of the color are specified
    /// in the design specification of the variant. Usually colorful.
    /// </summary>
    public readonly TonalPalette PrimaryPalette = primaryPalette;

    /// <summary>
    /// Given a tone, produces a color. Hue and chroma of the color are specified
    /// in the design specification of the variant. Usually less colorful.
    /// </summary>
    public readonly TonalPalette SecondaryPalette = secondaryPalette;

    /// <summary>
    /// Given a tone, produces a color. Hue and chroma of the color are specified
    /// in the design specification of the variant. Usually a different hue from
    /// primary and colorful.
    /// </summary>
    public readonly TonalPalette TertiaryPalette = tertiaryPalette;

    /// <summary>
    /// Given a tone, produces a color. Hue and chroma of the color are specified
    /// in the design specification of the variant. Usually not colorful at all,
    /// intended for background & surface colors.
    /// </summary>
    public readonly TonalPalette NeutralPalette = neutralPalette;

    /// <summary>
    /// Given a tone, produces a color. Hue and chroma of the color are specified
    /// in the design specification of the variant. Usually not colorful, but
    /// slightly more colorful than Neutral. Intended for backgrounds & surfaces.
    /// </summary>
    public readonly TonalPalette NeutralVariantPalette = neutralVariantPalette;

    /// <summary>
    /// Given a tone, produces a reddish, colorful, color.
    /// </summary>
    public readonly TonalPalette ErrorPalette = errorPalette ?? new TonalPalette(25.0, 84.0);

    public static double GetRotatedHue(Hct sourceColor, double[] hues, double[] rotations)
    {
        var sourceHue = sourceColor.Hue;
        if (hues.Length != rotations.Length)
            throw new ArgumentException("hues and rotations must have the same length");

        if (rotations.Length == 1)
            return MathUtils.SanitizeDegrees(sourceColor.Hue + rotations[0]);

        var size = hues.Length;
        for (var i = 0; i <= size - 2; i++)
        {
            var thisHue = hues[i];
            var nextHue = hues[i + 1];
            if (thisHue < sourceHue && sourceHue < nextHue)
                return MathUtils.SanitizeDegrees(sourceHue + rotations[i]);
        }

        // If this statement executes, something is wrong, there should have been a rotation
        // found using the arrays.
        return sourceHue;
    }

    public ArgbColor GetArgb(DynamicColor dynamicColor)
    {
        return dynamicColor.GetArgb(this);
    }

    public Hct GetHct(DynamicColor dynamicColor)
    {
        return dynamicColor.GetHct(this);
    }

    public ArgbColor PrimaryPaletteKeyColor =>
        GetArgb(MaterialDynamicColors.PrimaryPaletteKeyColor);

    public ArgbColor SecondaryPaletteKeyColor =>
        GetArgb(MaterialDynamicColors.SecondaryPaletteKeyColor);

    public ArgbColor TertiaryPaletteKeyColor =>
        GetArgb(MaterialDynamicColors.TertiaryPaletteKeyColor);

    public ArgbColor NeutralPaletteKeyColor =>
        GetArgb(MaterialDynamicColors.NeutralPaletteKeyColor);

    public ArgbColor NeutralVariantPaletteKeyColor =>
        GetArgb(MaterialDynamicColors.NeutralVariantPaletteKeyColor);

    public ArgbColor Background => GetArgb(MaterialDynamicColors.Background);
    public ArgbColor OnBackground => GetArgb(MaterialDynamicColors.OnBackground);
    public ArgbColor Surface => GetArgb(MaterialDynamicColors.Surface);
    public ArgbColor SurfaceDim => GetArgb(MaterialDynamicColors.SurfaceDim);
    public ArgbColor SurfaceBright => GetArgb(MaterialDynamicColors.SurfaceBright);

    public ArgbColor SurfaceContainerLowest =>
        GetArgb(MaterialDynamicColors.SurfaceContainerLowest);

    public ArgbColor SurfaceContainerLow => GetArgb(MaterialDynamicColors.SurfaceContainerLow);
    public ArgbColor SurfaceContainer => GetArgb(MaterialDynamicColors.SurfaceContainer);
    public ArgbColor SurfaceContainerHigh => GetArgb(MaterialDynamicColors.SurfaceContainerHigh);

    public ArgbColor SurfaceContainerHighest =>
        GetArgb(MaterialDynamicColors.SurfaceContainerHighest);

    public ArgbColor OnSurface => GetArgb(MaterialDynamicColors.OnSurface);
    public ArgbColor SurfaceVariant => GetArgb(MaterialDynamicColors.SurfaceVariant);
    public ArgbColor OnSurfaceVariant => GetArgb(MaterialDynamicColors.OnSurfaceVariant);
    public ArgbColor InverseSurface => GetArgb(MaterialDynamicColors.InverseSurface);
    public ArgbColor InverseOnSurface => GetArgb(MaterialDynamicColors.InverseOnSurface);
    public ArgbColor Outline => GetArgb(MaterialDynamicColors.Outline);
    public ArgbColor OutlineVariant => GetArgb(MaterialDynamicColors.OutlineVariant);
    public ArgbColor Shadow => GetArgb(MaterialDynamicColors.Shadow);
    public ArgbColor Scrim => GetArgb(MaterialDynamicColors.Scrim);
    public ArgbColor SurfaceTint => GetArgb(MaterialDynamicColors.SurfaceTint);
    public ArgbColor Primary => GetArgb(MaterialDynamicColors.Primary);
    public ArgbColor OnPrimary => GetArgb(MaterialDynamicColors.OnPrimary);
    public ArgbColor PrimaryContainer => GetArgb(MaterialDynamicColors.PrimaryContainer);
    public ArgbColor OnPrimaryContainer => GetArgb(MaterialDynamicColors.OnPrimaryContainer);
    public ArgbColor InversePrimary => GetArgb(MaterialDynamicColors.InversePrimary);
    public ArgbColor Secondary => GetArgb(MaterialDynamicColors.Secondary);
    public ArgbColor OnSecondary => GetArgb(MaterialDynamicColors.OnSecondary);
    public ArgbColor SecondaryContainer => GetArgb(MaterialDynamicColors.SecondaryContainer);
    public ArgbColor OnSecondaryContainer => GetArgb(MaterialDynamicColors.OnSecondaryContainer);
    public ArgbColor Tertiary => GetArgb(MaterialDynamicColors.Tertiary);
    public ArgbColor OnTertiary => GetArgb(MaterialDynamicColors.OnTertiary);
    public ArgbColor TertiaryContainer => GetArgb(MaterialDynamicColors.TertiaryContainer);
    public ArgbColor OnTertiaryContainer => GetArgb(MaterialDynamicColors.OnTertiaryContainer);
    public ArgbColor Error => GetArgb(MaterialDynamicColors.Error);
    public ArgbColor OnError => GetArgb(MaterialDynamicColors.OnError);
    public ArgbColor ErrorContainer => GetArgb(MaterialDynamicColors.ErrorContainer);
    public ArgbColor OnErrorContainer => GetArgb(MaterialDynamicColors.OnErrorContainer);
    public ArgbColor PrimaryFixed => GetArgb(MaterialDynamicColors.PrimaryFixed);
    public ArgbColor PrimaryFixedDim => GetArgb(MaterialDynamicColors.PrimaryFixedDim);
    public ArgbColor OnPrimaryFixed => GetArgb(MaterialDynamicColors.OnPrimaryFixed);
    public ArgbColor OnPrimaryFixedVariant => GetArgb(MaterialDynamicColors.OnPrimaryFixedVariant);
    public ArgbColor SecondaryFixed => GetArgb(MaterialDynamicColors.SecondaryFixed);
    public ArgbColor SecondaryFixedDim => GetArgb(MaterialDynamicColors.SecondaryFixedDim);
    public ArgbColor OnSecondaryFixed => GetArgb(MaterialDynamicColors.OnSecondaryFixed);

    public ArgbColor OnSecondaryFixedVariant =>
        GetArgb(MaterialDynamicColors.OnSecondaryFixedVariant);

    public ArgbColor TertiaryFixed => GetArgb(MaterialDynamicColors.TertiaryFixed);
    public ArgbColor TertiaryFixedDim => GetArgb(MaterialDynamicColors.TertiaryFixedDim);
    public ArgbColor OnTertiaryFixed => GetArgb(MaterialDynamicColors.OnTertiaryFixed);

    public ArgbColor OnTertiaryFixedVariant =>
        GetArgb(MaterialDynamicColors.OnTertiaryFixedVariant);
}