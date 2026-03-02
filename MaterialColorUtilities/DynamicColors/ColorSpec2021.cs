// Copyright 2025 Google LLC
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

using MaterialColorUtilities.Dislike;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Temperature;
using MaterialColorUtilities.Utils;

/// <summary>
/// Spec 2021 dynamic color behavior.
/// </summary>
public class ColorSpec2021 : ColorSpec
{
    private static readonly DynamicColor ErrorPaletteKeyColorValue = DynamicColor.FromPalette(
        "error_palette_key_color",
        s => s.ErrorPalette,
        s => s.ErrorPalette.KeyColor.Tone
    );

    public virtual DynamicColor HighestSurface(DynamicScheme scheme) => scheme.IsDark ? SurfaceBright : SurfaceDim;

    public virtual DynamicColor PrimaryPaletteKeyColor => LegacyMaterialDynamicColors.PrimaryPaletteKeyColor;

    public virtual DynamicColor SecondaryPaletteKeyColor => LegacyMaterialDynamicColors.SecondaryPaletteKeyColor;

    public virtual DynamicColor TertiaryPaletteKeyColor => LegacyMaterialDynamicColors.TertiaryPaletteKeyColor;

    public virtual DynamicColor NeutralPaletteKeyColor => LegacyMaterialDynamicColors.NeutralPaletteKeyColor;

    public virtual DynamicColor NeutralVariantPaletteKeyColor => LegacyMaterialDynamicColors.NeutralVariantPaletteKeyColor;

    public virtual DynamicColor ErrorPaletteKeyColor => ErrorPaletteKeyColorValue;

    public virtual DynamicColor Background => LegacyMaterialDynamicColors.Background;

    public virtual DynamicColor OnBackground => LegacyMaterialDynamicColors.OnBackground;

    public virtual DynamicColor Surface => LegacyMaterialDynamicColors.Surface;

    public virtual DynamicColor SurfaceDim => LegacyMaterialDynamicColors.SurfaceDim;

    public virtual DynamicColor SurfaceBright => LegacyMaterialDynamicColors.SurfaceBright;

    public virtual DynamicColor SurfaceContainerLowest => LegacyMaterialDynamicColors.SurfaceContainerLowest;

    public virtual DynamicColor SurfaceContainerLow => LegacyMaterialDynamicColors.SurfaceContainerLow;

    public virtual DynamicColor SurfaceContainer => LegacyMaterialDynamicColors.SurfaceContainer;

    public virtual DynamicColor SurfaceContainerHigh => LegacyMaterialDynamicColors.SurfaceContainerHigh;

    public virtual DynamicColor SurfaceContainerHighest => LegacyMaterialDynamicColors.SurfaceContainerHighest;

    public virtual DynamicColor OnSurface => LegacyMaterialDynamicColors.OnSurface;

    public virtual DynamicColor SurfaceVariant => LegacyMaterialDynamicColors.SurfaceVariant;

    public virtual DynamicColor OnSurfaceVariant => LegacyMaterialDynamicColors.OnSurfaceVariant;

    public virtual DynamicColor InverseSurface => LegacyMaterialDynamicColors.InverseSurface;

    public virtual DynamicColor InverseOnSurface => LegacyMaterialDynamicColors.InverseOnSurface;

    public virtual DynamicColor Outline => LegacyMaterialDynamicColors.Outline;

    public virtual DynamicColor OutlineVariant => LegacyMaterialDynamicColors.OutlineVariant;

    public virtual DynamicColor Shadow => LegacyMaterialDynamicColors.Shadow;

    public virtual DynamicColor Scrim => LegacyMaterialDynamicColors.Scrim;

    public virtual DynamicColor SurfaceTint => LegacyMaterialDynamicColors.SurfaceTint;

    public virtual DynamicColor Primary => LegacyMaterialDynamicColors.Primary;

    public virtual DynamicColor? PrimaryDim => null;

    public virtual DynamicColor OnPrimary => LegacyMaterialDynamicColors.OnPrimary;

    public virtual DynamicColor PrimaryContainer => LegacyMaterialDynamicColors.PrimaryContainer;

    public virtual DynamicColor OnPrimaryContainer => LegacyMaterialDynamicColors.OnPrimaryContainer;

    public virtual DynamicColor InversePrimary => LegacyMaterialDynamicColors.InversePrimary;

    public virtual DynamicColor Secondary => LegacyMaterialDynamicColors.Secondary;

    public virtual DynamicColor? SecondaryDim => null;

    public virtual DynamicColor OnSecondary => LegacyMaterialDynamicColors.OnSecondary;

    public virtual DynamicColor SecondaryContainer => LegacyMaterialDynamicColors.SecondaryContainer;

    public virtual DynamicColor OnSecondaryContainer => LegacyMaterialDynamicColors.OnSecondaryContainer;

    public virtual DynamicColor Tertiary => LegacyMaterialDynamicColors.Tertiary;

    public virtual DynamicColor? TertiaryDim => null;

    public virtual DynamicColor OnTertiary => LegacyMaterialDynamicColors.OnTertiary;

    public virtual DynamicColor TertiaryContainer => LegacyMaterialDynamicColors.TertiaryContainer;

    public virtual DynamicColor OnTertiaryContainer => LegacyMaterialDynamicColors.OnTertiaryContainer;

    public virtual DynamicColor Error => LegacyMaterialDynamicColors.Error;

    public virtual DynamicColor? ErrorDim => null;

    public virtual DynamicColor OnError => LegacyMaterialDynamicColors.OnError;

    public virtual DynamicColor ErrorContainer => LegacyMaterialDynamicColors.ErrorContainer;

    public virtual DynamicColor OnErrorContainer => LegacyMaterialDynamicColors.OnErrorContainer;

    public virtual DynamicColor PrimaryFixed => LegacyMaterialDynamicColors.PrimaryFixed;

    public virtual DynamicColor PrimaryFixedDim => LegacyMaterialDynamicColors.PrimaryFixedDim;

    public virtual DynamicColor OnPrimaryFixed => LegacyMaterialDynamicColors.OnPrimaryFixed;

    public virtual DynamicColor OnPrimaryFixedVariant => LegacyMaterialDynamicColors.OnPrimaryFixedVariant;

    public virtual DynamicColor SecondaryFixed => LegacyMaterialDynamicColors.SecondaryFixed;

    public virtual DynamicColor SecondaryFixedDim => LegacyMaterialDynamicColors.SecondaryFixedDim;

    public virtual DynamicColor OnSecondaryFixed => LegacyMaterialDynamicColors.OnSecondaryFixed;

    public virtual DynamicColor OnSecondaryFixedVariant => LegacyMaterialDynamicColors.OnSecondaryFixedVariant;

    public virtual DynamicColor TertiaryFixed => LegacyMaterialDynamicColors.TertiaryFixed;

    public virtual DynamicColor TertiaryFixedDim => LegacyMaterialDynamicColors.TertiaryFixedDim;

    public virtual DynamicColor OnTertiaryFixed => LegacyMaterialDynamicColors.OnTertiaryFixed;

    public virtual DynamicColor OnTertiaryFixedVariant => LegacyMaterialDynamicColors.OnTertiaryFixedVariant;

    public virtual Hct GetHct(DynamicScheme scheme, DynamicColor color)
    {
        var tone = GetTone(scheme, color);
        var palette = color.Palette(scheme);
        var chromaMultiplier = color.ChromaMultiplier?.Invoke(scheme) ?? 1.0;
        if (Math.Abs(chromaMultiplier - 1.0) < 0.000_001)
            return palette.GetHct(tone);
        return Hct.From(palette.Hue, palette.Chroma * chromaMultiplier, tone);
    }

    private static bool IsLegacyNearer(TonePolarity polarity, bool isDark)
    {
        return polarity == TonePolarity.Nearer
            || polarity == TonePolarity.RelativeLighter
            || (polarity == TonePolarity.Lighter && !isDark)
            || (polarity == TonePolarity.Darker && isDark)
            || (polarity == TonePolarity.RelativeDarker && isDark);
    }

    public virtual double GetTone(DynamicScheme scheme, DynamicColor color)
    {
        var decreasingContrast = scheme.ContrastLevel < 0;
        var toneDeltaPair = color.ToneDeltaPair?.Invoke(scheme);

        if (toneDeltaPair != null)
        {
            var roleA = toneDeltaPair.RoleA;
            var roleB = toneDeltaPair.RoleB;
            var delta = toneDeltaPair.Delta;
            var polarity = toneDeltaPair.Polarity;
            var stayTogether = toneDeltaPair.StayTogether;

            var aIsNearer = IsLegacyNearer(polarity, scheme.IsDark);
            if (toneDeltaPair.Constraint == ToneDeltaPair.DeltaConstraint.Nearer)
                aIsNearer = true;
            if (toneDeltaPair.Constraint == ToneDeltaPair.DeltaConstraint.Farther)
                aIsNearer = false;

            var nearer = aIsNearer ? roleA : roleB;
            var farther = aIsNearer ? roleB : roleA;
            var amNearer = color.Name == nearer.Name;
            var expansionDir = scheme.IsDark ? 1.0 : -1.0;
            var nTone = nearer.Tone(scheme);
            var fTone = farther.Tone(scheme);

            var bg = color.Background?.Invoke(scheme);
            var nContrastCurve = nearer.ContrastCurve?.Invoke(scheme);
            var fContrastCurve = farther.ContrastCurve?.Invoke(scheme);

            if (bg != null && nContrastCurve != null && fContrastCurve != null)
            {
                var nContrast = nContrastCurve.Get(scheme.ContrastLevel);
                var fContrast = fContrastCurve.Get(scheme.ContrastLevel);
                var bgTone = bg.GetTone(scheme);

                if (Contrast.Contrast.RatioOfTones(bgTone, nTone) < nContrast)
                    nTone = DynamicColor.ForegroundTone(bgTone, nContrast);
                if (Contrast.Contrast.RatioOfTones(bgTone, fTone) < fContrast)
                    fTone = DynamicColor.ForegroundTone(bgTone, fContrast);

                if (decreasingContrast)
                {
                    nTone = DynamicColor.ForegroundTone(bgTone, nContrast);
                    fTone = DynamicColor.ForegroundTone(bgTone, fContrast);
                }
            }

            if ((fTone - nTone) * expansionDir < delta)
            {
                if (toneDeltaPair.Constraint == ToneDeltaPair.DeltaConstraint.Nearer)
                {
                    nTone = Math.Clamp(fTone - delta * expansionDir, 0, 100);
                }
                else
                {
                    fTone = Math.Clamp(nTone + delta * expansionDir, 0, 100);
                    if ((fTone - nTone) * expansionDir < delta)
                        nTone = Math.Clamp(fTone - delta * expansionDir, 0, 100);
                }
            }

            if (nTone is >= 50 and < 60)
            {
                if (expansionDir > 0)
                {
                    nTone = 60;
                    fTone = Math.Max(fTone, nTone + delta * expansionDir);
                }
                else
                {
                    nTone = 49;
                    fTone = Math.Min(fTone, nTone + delta * expansionDir);
                }
            }
            else if (fTone is >= 50 and < 60)
            {
                if (stayTogether)
                {
                    if (expansionDir > 0)
                    {
                        nTone = 60;
                        fTone = Math.Max(fTone, nTone + delta * expansionDir);
                    }
                    else
                    {
                        nTone = 49;
                        fTone = Math.Min(fTone, nTone + delta * expansionDir);
                    }
                }
                else
                {
                    fTone = expansionDir > 0 ? 60 : 49;
                }
            }

            return amNearer ? nTone : fTone;
        }

        var answer = color.Tone(scheme);
        var background = color.Background?.Invoke(scheme);
        var contrastCurve = color.ContrastCurve?.Invoke(scheme);
        if (background == null || contrastCurve == null)
            return answer;

        var bgToneSingle = background.GetTone(scheme);
        var desiredRatio = contrastCurve.Get(scheme.ContrastLevel);

        if (Contrast.Contrast.RatioOfTones(bgToneSingle, answer) < desiredRatio)
            answer = DynamicColor.ForegroundTone(bgToneSingle, desiredRatio);
        if (decreasingContrast)
            answer = DynamicColor.ForegroundTone(bgToneSingle, desiredRatio);

        if (color.IsBackground && answer is >= 50 and < 60)
            answer = Contrast.Contrast.RatioOfTones(49, bgToneSingle) >= desiredRatio ? 49 : 60;

        var secondBackground = color.SecondBackground?.Invoke(scheme);
        if (secondBackground == null)
            return answer;

        var bgTone1 = background.GetTone(scheme);
        var bgTone2 = secondBackground.GetTone(scheme);
        var upper = Math.Max(bgTone1, bgTone2);
        var lower = Math.Min(bgTone1, bgTone2);

        if (
            Contrast.Contrast.RatioOfTones(upper, answer) >= desiredRatio
            && Contrast.Contrast.RatioOfTones(lower, answer) >= desiredRatio
        )
            return answer;

        var lightOption = Contrast.Contrast.Lighter(upper, desiredRatio);
        var darkOption = Contrast.Contrast.Darker(lower, desiredRatio);
        var availables = new List<double>();
        if (lightOption != -1)
            availables.Add(lightOption);
        if (darkOption != -1)
            availables.Add(darkOption);

        var prefersLight =
            DynamicColor.TonePrefersLightForeground(bgTone1)
            || DynamicColor.TonePrefersLightForeground(bgTone2);

        if (prefersLight)
            return lightOption == -1 ? 100 : lightOption;
        if (availables.Count == 1)
            return availables[0];
        return darkOption == -1 ? 0 : darkOption;
    }

    public virtual TonalPalette GetPrimaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity => new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma),
            Variant.FruitSalad => new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue - 50.0), 48.0),
            Variant.Monochrome => new TonalPalette(sourceColorHct.Hue, 0.0),
            Variant.Neutral => new TonalPalette(sourceColorHct.Hue, 12.0),
            Variant.Rainbow => new TonalPalette(sourceColorHct.Hue, 48.0),
            Variant.TonalSpot => new TonalPalette(sourceColorHct.Hue, 36.0),
            Variant.Expressive => new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 240), 40),
            Variant.Vibrant => new TonalPalette(sourceColorHct.Hue, 200.0),
            _ => throw new ArgumentException($"{variant} variant is not supported in this spec.")
        };
    }

    public virtual TonalPalette GetSecondaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity => new TonalPalette(
                sourceColorHct.Hue,
                Math.Max(sourceColorHct.Chroma - 32.0, sourceColorHct.Chroma * 0.5)
            ),
            Variant.FruitSalad => new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue - 50.0), 36.0),
            Variant.Monochrome => new TonalPalette(sourceColorHct.Hue, 0.0),
            Variant.Neutral => new TonalPalette(sourceColorHct.Hue, 8.0),
            Variant.Rainbow => new TonalPalette(sourceColorHct.Hue, 16.0),
            Variant.TonalSpot => new TonalPalette(sourceColorHct.Hue, 16.0),
            Variant.Expressive => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0, 21, 51, 121, 151, 191, 271, 321, 360],
                    [45, 95, 45, 20, 45, 90, 45, 45, 45]
                ),
                24.0
            ),
            Variant.Vibrant => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0, 41, 61, 101, 131, 181, 251, 301, 360],
                    [18, 15, 10, 12, 15, 18, 15, 12, 12]
                ),
                24.0
            ),
            _ => throw new ArgumentException($"{variant} variant is not supported in this spec.")
        };
    }

    public virtual TonalPalette GetTertiaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        return variant switch
        {
            Variant.Content => new TonalPalette(
                DislikeAnalyzer.FixIfDisliked(new TemperatureCache(sourceColorHct).Analogous(3, 6).Last())
            ),
            Variant.Fidelity => new TonalPalette(
                DislikeAnalyzer.FixIfDisliked(new TemperatureCache(sourceColorHct).Complement!.Value)
            ),
            Variant.FruitSalad => new TonalPalette(sourceColorHct.Hue, 36.0),
            Variant.Monochrome => new TonalPalette(sourceColorHct.Hue, 0.0),
            Variant.Neutral => new TonalPalette(sourceColorHct.Hue, 16.0),
            Variant.Rainbow => new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 60.0), 24.0),
            Variant.TonalSpot => new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 60.0), 24.0),
            Variant.Expressive => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0, 21, 51, 121, 151, 191, 271, 321, 360],
                    [120, 120, 20, 45, 20, 15, 20, 120, 120]
                ),
                32.0
            ),
            Variant.Vibrant => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0, 41, 61, 101, 131, 181, 251, 301, 360],
                    [35, 30, 20, 25, 30, 35, 30, 25, 25]
                ),
                32.0
            ),
            _ => throw new ArgumentException($"{variant} variant is not supported in this spec.")
        };
    }

    public virtual TonalPalette GetNeutralPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity => new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma / 8.0),
            Variant.FruitSalad => new TonalPalette(sourceColorHct.Hue, 10.0),
            Variant.Monochrome => new TonalPalette(sourceColorHct.Hue, 0.0),
            Variant.Neutral => new TonalPalette(sourceColorHct.Hue, 2.0),
            Variant.Rainbow => new TonalPalette(sourceColorHct.Hue, 0.0),
            Variant.TonalSpot => new TonalPalette(sourceColorHct.Hue, 6.0),
            Variant.Expressive => new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 15.0), 8.0),
            Variant.Vibrant => new TonalPalette(sourceColorHct.Hue, 10.0),
            _ => throw new ArgumentException($"{variant} variant is not supported in this spec.")
        };
    }

    public virtual TonalPalette GetNeutralVariantPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        return variant switch
        {
            Variant.Content or Variant.Fidelity => new TonalPalette(
                sourceColorHct.Hue,
                sourceColorHct.Chroma / 8.0 + 4.0
            ),
            Variant.FruitSalad => new TonalPalette(sourceColorHct.Hue, 16.0),
            Variant.Monochrome => new TonalPalette(sourceColorHct.Hue, 0.0),
            Variant.Neutral => new TonalPalette(sourceColorHct.Hue, 2.0),
            Variant.Rainbow => new TonalPalette(sourceColorHct.Hue, 0.0),
            Variant.TonalSpot => new TonalPalette(sourceColorHct.Hue, 8.0),
            Variant.Expressive => new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 15.0), 12.0),
            Variant.Vibrant => new TonalPalette(sourceColorHct.Hue, 12.0),
            _ => throw new ArgumentException($"{variant} variant is not supported in this spec.")
        };
    }

    public virtual TonalPalette? GetErrorPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        return new TonalPalette(25.0, 84.0);
    }
}
