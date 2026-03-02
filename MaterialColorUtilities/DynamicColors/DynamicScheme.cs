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

namespace MaterialColorUtilities.DynamicColors;

using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

/// <summary>
/// Provides settings and palettes required to resolve dynamic colors.
/// </summary>
public class DynamicScheme
{
    public enum Platform
    {
        Phone,
        Watch
    }

    public static readonly ColorSpec.SpecVersion DefaultSpecVersion = ColorSpec.SpecVersion.Spec2021;
    public static readonly Platform DefaultPlatform = Platform.Phone;

    private static readonly MaterialDynamicColors Roles = new();

    public readonly ArgbColor SourceColorArgb;
    public readonly Hct SourceColorHct;
    public readonly IReadOnlyList<Hct> SourceColorHctList;
    public readonly Variant Variant;
    public readonly bool IsDark;
    public readonly Platform SchemePlatform;
    public Platform PlatformType => SchemePlatform;
    public readonly double ContrastLevel;
    public readonly ColorSpec.SpecVersion SpecVersion;

    public readonly TonalPalette PrimaryPalette;
    public readonly TonalPalette SecondaryPalette;
    public readonly TonalPalette TertiaryPalette;
    public readonly TonalPalette NeutralPalette;
    public readonly TonalPalette NeutralVariantPalette;
    public readonly TonalPalette ErrorPalette;

    public DynamicScheme(
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
        : this(
            [sourceColorHct],
            variant,
            isDark,
            contrastLevel,
            DefaultPlatform,
            DefaultSpecVersion,
            primaryPalette,
            secondaryPalette,
            tertiaryPalette,
            neutralPalette,
            neutralVariantPalette,
            errorPalette
        )
    {
    }

    public DynamicScheme(
        Hct sourceColorHct,
        Variant variant,
        bool isDark,
        double contrastLevel,
        Platform platform,
        ColorSpec.SpecVersion specVersion,
        TonalPalette primaryPalette,
        TonalPalette secondaryPalette,
        TonalPalette tertiaryPalette,
        TonalPalette neutralPalette,
        TonalPalette neutralVariantPalette,
        TonalPalette? errorPalette = null
    )
        : this(
            [sourceColorHct],
            variant,
            isDark,
            contrastLevel,
            platform,
            specVersion,
            primaryPalette,
            secondaryPalette,
            tertiaryPalette,
            neutralPalette,
            neutralVariantPalette,
            errorPalette
        )
    {
    }

    public DynamicScheme(
        IReadOnlyList<Hct> sourceColorHctList,
        Variant variant,
        bool isDark,
        double contrastLevel,
        Platform platform,
        ColorSpec.SpecVersion specVersion,
        TonalPalette primaryPalette,
        TonalPalette secondaryPalette,
        TonalPalette tertiaryPalette,
        TonalPalette neutralPalette,
        TonalPalette neutralVariantPalette,
        TonalPalette? errorPalette = null
    )
    {
        if (sourceColorHctList.Count == 0)
            throw new ArgumentException("sourceColorHctList cannot be empty", nameof(sourceColorHctList));

        SourceColorHctList = sourceColorHctList.ToArray();
        SourceColorHct = SourceColorHctList[0];
        SourceColorArgb = SourceColorHct.Argb;
        Variant = variant;
        IsDark = isDark;
        ContrastLevel = contrastLevel;
        SchemePlatform = platform;
        SpecVersion = MaybeFallbackSpecVersion(specVersion, variant);

        PrimaryPalette = primaryPalette;
        SecondaryPalette = secondaryPalette;
        TertiaryPalette = tertiaryPalette;
        NeutralPalette = neutralPalette;
        NeutralVariantPalette = neutralVariantPalette;
        ErrorPalette = errorPalette ?? new TonalPalette(25.0, 84.0);
    }

    public static DynamicScheme From(DynamicScheme other, bool isDark)
    {
        return From(other, isDark, other.ContrastLevel);
    }

    public static DynamicScheme From(DynamicScheme other, bool isDark, double contrastLevel)
    {
        return new DynamicScheme(
            other.SourceColorHctList,
            other.Variant,
            isDark,
            contrastLevel,
            other.SchemePlatform,
            other.SpecVersion,
            other.PrimaryPalette,
            other.SecondaryPalette,
            other.TertiaryPalette,
            other.NeutralPalette,
            other.NeutralVariantPalette,
            other.ErrorPalette
        );
    }

    private static ColorSpec.SpecVersion MaybeFallbackSpecVersion(
        ColorSpec.SpecVersion specVersion,
        Variant variant
    )
    {
        if (variant == Variant.Cmf)
            return specVersion;

        if (
            variant == Variant.Expressive
            || variant == Variant.Vibrant
            || variant == Variant.TonalSpot
            || variant == Variant.Neutral
        )
        {
            return specVersion == ColorSpec.SpecVersion.Spec2026
                ? ColorSpec.SpecVersion.Spec2025
                : specVersion;
        }

        return ColorSpec.SpecVersion.Spec2021;
    }

    public static double GetPiecewiseValue(Hct sourceColor, double[] hueBreakpoints, double[] values)
    {
        var size = Math.Min(hueBreakpoints.Length - 1, values.Length);
        var sourceHue = sourceColor.Hue;
        for (var i = 0; i < size; i++)
        {
            if (sourceHue >= hueBreakpoints[i] && sourceHue < hueBreakpoints[i + 1])
                return MathUtils.SanitizeDegrees(values[i]);
        }

        return sourceHue;
    }

    public static double GetRotatedHue(Hct sourceColor, double[] hueBreakpoints, double[] rotations)
    {
        var rotation = GetPiecewiseValue(sourceColor, hueBreakpoints, rotations);
        if (Math.Min(hueBreakpoints.Length - 1, rotations.Length) <= 0)
            rotation = 0;
        return MathUtils.SanitizeDegrees(sourceColor.Hue + rotation);
    }

    public ArgbColor GetArgb(DynamicColor dynamicColor)
    {
        return dynamicColor.GetArgb(this);
    }

    public Hct GetHct(DynamicColor dynamicColor)
    {
        return dynamicColor.GetHct(this);
    }

    public ArgbColor PrimaryPaletteKeyColor => GetArgb(Roles.PrimaryPaletteKeyColor);
    public ArgbColor SecondaryPaletteKeyColor => GetArgb(Roles.SecondaryPaletteKeyColor);
    public ArgbColor TertiaryPaletteKeyColor => GetArgb(Roles.TertiaryPaletteKeyColor);
    public ArgbColor NeutralPaletteKeyColor => GetArgb(Roles.NeutralPaletteKeyColor);
    public ArgbColor NeutralVariantPaletteKeyColor => GetArgb(Roles.NeutralVariantPaletteKeyColor);
    public ArgbColor ErrorPaletteKeyColor => GetArgb(Roles.ErrorPaletteKeyColor);
    public ArgbColor Background => GetArgb(Roles.Background);
    public ArgbColor OnBackground => GetArgb(Roles.OnBackground);
    public ArgbColor Surface => GetArgb(Roles.Surface);
    public ArgbColor SurfaceDim => GetArgb(Roles.SurfaceDim);
    public ArgbColor SurfaceBright => GetArgb(Roles.SurfaceBright);
    public ArgbColor SurfaceContainerLowest => GetArgb(Roles.SurfaceContainerLowest);
    public ArgbColor SurfaceContainerLow => GetArgb(Roles.SurfaceContainerLow);
    public ArgbColor SurfaceContainer => GetArgb(Roles.SurfaceContainer);
    public ArgbColor SurfaceContainerHigh => GetArgb(Roles.SurfaceContainerHigh);
    public ArgbColor SurfaceContainerHighest => GetArgb(Roles.SurfaceContainerHighest);
    public ArgbColor OnSurface => GetArgb(Roles.OnSurface);
    public ArgbColor SurfaceVariant => GetArgb(Roles.SurfaceVariant);
    public ArgbColor OnSurfaceVariant => GetArgb(Roles.OnSurfaceVariant);
    public ArgbColor InverseSurface => GetArgb(Roles.InverseSurface);
    public ArgbColor InverseOnSurface => GetArgb(Roles.InverseOnSurface);
    public ArgbColor Outline => GetArgb(Roles.Outline);
    public ArgbColor OutlineVariant => GetArgb(Roles.OutlineVariant);
    public ArgbColor Shadow => GetArgb(Roles.Shadow);
    public ArgbColor Scrim => GetArgb(Roles.Scrim);
    public ArgbColor SurfaceTint => GetArgb(Roles.SurfaceTint);
    public ArgbColor Primary => GetArgb(Roles.Primary);
    public ArgbColor PrimaryDim => GetArgb(Roles.PrimaryDim ?? Roles.Primary);
    public ArgbColor OnPrimary => GetArgb(Roles.OnPrimary);
    public ArgbColor PrimaryContainer => GetArgb(Roles.PrimaryContainer);
    public ArgbColor OnPrimaryContainer => GetArgb(Roles.OnPrimaryContainer);
    public ArgbColor InversePrimary => GetArgb(Roles.InversePrimary);
    public ArgbColor Secondary => GetArgb(Roles.Secondary);
    public ArgbColor SecondaryDim => GetArgb(Roles.SecondaryDim ?? Roles.Secondary);
    public ArgbColor OnSecondary => GetArgb(Roles.OnSecondary);
    public ArgbColor SecondaryContainer => GetArgb(Roles.SecondaryContainer);
    public ArgbColor OnSecondaryContainer => GetArgb(Roles.OnSecondaryContainer);
    public ArgbColor Tertiary => GetArgb(Roles.Tertiary);
    public ArgbColor TertiaryDim => GetArgb(Roles.TertiaryDim ?? Roles.Tertiary);
    public ArgbColor OnTertiary => GetArgb(Roles.OnTertiary);
    public ArgbColor TertiaryContainer => GetArgb(Roles.TertiaryContainer);
    public ArgbColor OnTertiaryContainer => GetArgb(Roles.OnTertiaryContainer);
    public ArgbColor Error => GetArgb(Roles.Error);
    public ArgbColor ErrorDim => GetArgb(Roles.ErrorDim ?? Roles.Error);
    public ArgbColor OnError => GetArgb(Roles.OnError);
    public ArgbColor ErrorContainer => GetArgb(Roles.ErrorContainer);
    public ArgbColor OnErrorContainer => GetArgb(Roles.OnErrorContainer);
    public ArgbColor PrimaryFixed => GetArgb(Roles.PrimaryFixed);
    public ArgbColor PrimaryFixedDim => GetArgb(Roles.PrimaryFixedDim);
    public ArgbColor OnPrimaryFixed => GetArgb(Roles.OnPrimaryFixed);
    public ArgbColor OnPrimaryFixedVariant => GetArgb(Roles.OnPrimaryFixedVariant);
    public ArgbColor SecondaryFixed => GetArgb(Roles.SecondaryFixed);
    public ArgbColor SecondaryFixedDim => GetArgb(Roles.SecondaryFixedDim);
    public ArgbColor OnSecondaryFixed => GetArgb(Roles.OnSecondaryFixed);
    public ArgbColor OnSecondaryFixedVariant => GetArgb(Roles.OnSecondaryFixedVariant);
    public ArgbColor TertiaryFixed => GetArgb(Roles.TertiaryFixed);
    public ArgbColor TertiaryFixedDim => GetArgb(Roles.TertiaryFixedDim);
    public ArgbColor OnTertiaryFixed => GetArgb(Roles.OnTertiaryFixed);
    public ArgbColor OnTertiaryFixedVariant => GetArgb(Roles.OnTertiaryFixedVariant);
}
