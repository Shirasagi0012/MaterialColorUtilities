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

using MaterialColorUtilities.Dislike;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.DynamicColor;

/// <summary>
/// Tokens, or named colors, in the Material Design system.
/// </summary>
public static class MaterialDynamicColors
{
    public const double ContentAccentToneDelta = 15.0;

    public static bool IsFidelity(DynamicScheme scheme)
    {
        return scheme.Variant == Variant.Fidelity || scheme.Variant == Variant.Content;
    }

    public static bool IsMonochrome(DynamicScheme scheme)
    {
        return scheme.Variant == Variant.Monochrome;
    }

    public static DynamicColor HighestSurface(DynamicScheme s)
    {
        return s.IsDark ? SurfaceBright : SurfaceDim;
    }

    public static readonly DynamicColor PrimaryPaletteKeyColor = DynamicColor.FromPalette(
        "primary_palette_key_color",
        s => s.PrimaryPalette,
        s => s.PrimaryPalette.KeyColor.Tone
    );

    public static readonly DynamicColor SecondaryPaletteKeyColor = DynamicColor.FromPalette(
        "secondary_palette_key_color",
        s => s.SecondaryPalette,
        s => s.SecondaryPalette.KeyColor.Tone
    );

    public static readonly DynamicColor TertiaryPaletteKeyColor = DynamicColor.FromPalette(
        "tertiary_palette_key_color",
        s => s.TertiaryPalette,
        s => s.TertiaryPalette.KeyColor.Tone
    );

    public static readonly DynamicColor NeutralPaletteKeyColor = DynamicColor.FromPalette(
        "neutral_palette_key_color",
        s => s.NeutralPalette,
        s => s.NeutralPalette.KeyColor.Tone
    );

    public static readonly DynamicColor NeutralVariantPaletteKeyColor = DynamicColor.FromPalette(
        "neutral_variant_palette_key_color",
        s => s.NeutralVariantPalette,
        s => s.NeutralVariantPalette.KeyColor.Tone
    );

    public static readonly DynamicColor Background = DynamicColor.FromPalette(
        "background",
        s => s.NeutralPalette,
        s => s.IsDark ? 6 : 98,
        true
    );

    public static readonly DynamicColor OnBackground = DynamicColor.FromPalette(
        "on_background",
        s => s.NeutralPalette,
        s => s.IsDark ? 90 : 10,
        background: s => Background,
        contrastCurve: new ContrastCurve(3, 3, 4.5, 7)
    );

    public static readonly DynamicColor Surface = DynamicColor.FromPalette(
        "surface",
        s => s.NeutralPalette,
        s => s.IsDark ? 6 : 98,
        true
    );

    public static readonly DynamicColor SurfaceDim = DynamicColor.FromPalette(
        "surface_dim",
        s => s.NeutralPalette,
        s => s.IsDark ? 6 : new ContrastCurve(87, 87, 80, 75).Get(s.ContrastLevel),
        true
    );

    public static readonly DynamicColor SurfaceBright = DynamicColor.FromPalette(
        "surface_bright",
        s => s.NeutralPalette,
        s => s.IsDark ? new ContrastCurve(24, 24, 29, 34).Get(s.ContrastLevel) : 98,
        true
    );

    public static readonly DynamicColor SurfaceContainerLowest = DynamicColor.FromPalette(
        "surface_container_lowest",
        s => s.NeutralPalette,
        s => s.IsDark ? new ContrastCurve(4, 4, 2, 0).Get(s.ContrastLevel) : 100,
        true
    );

    public static readonly DynamicColor SurfaceContainerLow = DynamicColor.FromPalette(
        "surface_container_low",
        s => s.NeutralPalette,
        s =>
            s.IsDark
                ? new ContrastCurve(10, 10, 11, 12).Get(s.ContrastLevel)
                : new ContrastCurve(96, 96, 96, 95).Get(s.ContrastLevel),
        true
    );

    public static readonly DynamicColor SurfaceContainer = DynamicColor.FromPalette(
        "surface_container",
        s => s.NeutralPalette,
        s =>
            s.IsDark
                ? new ContrastCurve(12, 12, 16, 20).Get(s.ContrastLevel)
                : new ContrastCurve(94, 94, 92, 90).Get(s.ContrastLevel),
        true
    );

    public static readonly DynamicColor SurfaceContainerHigh = DynamicColor.FromPalette(
        "surface_container_high",
        s => s.NeutralPalette,
        s =>
            s.IsDark
                ? new ContrastCurve(17, 17, 21, 25).Get(s.ContrastLevel)
                : new ContrastCurve(92, 92, 88, 85).Get(s.ContrastLevel),
        true
    );

    public static readonly DynamicColor SurfaceContainerHighest = DynamicColor.FromPalette(
        "surface_container_highest",
        s => s.NeutralPalette,
        s =>
            s.IsDark
                ? new ContrastCurve(22, 22, 26, 30).Get(s.ContrastLevel)
                : new ContrastCurve(90, 90, 84, 80).Get(s.ContrastLevel),
        true
    );

    public static readonly DynamicColor OnSurface = DynamicColor.FromPalette(
        "on_surface",
        s => s.NeutralPalette,
        s => s.IsDark ? 90 : 10,
        background: HighestSurface,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor SurfaceVariant = DynamicColor.FromPalette(
        "surface_variant",
        s => s.NeutralVariantPalette,
        s => s.IsDark ? 30 : 90,
        true
    );

    public static readonly DynamicColor OnSurfaceVariant = DynamicColor.FromPalette(
        "on_surface_variant",
        s => s.NeutralVariantPalette,
        s => s.IsDark ? 80 : 30,
        background: HighestSurface,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    public static readonly DynamicColor InverseSurface = DynamicColor.FromPalette(
        "inverse_surface",
        s => s.NeutralPalette,
        s => s.IsDark ? 90 : 20
    );

    public static readonly DynamicColor InverseOnSurface = DynamicColor.FromPalette(
        "inverse_on_surface",
        s => s.NeutralPalette,
        s => s.IsDark ? 20 : 95,
        background: s => InverseSurface,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor Outline = DynamicColor.FromPalette(
        "outline",
        s => s.NeutralVariantPalette,
        s => s.IsDark ? 60 : 50,
        background: HighestSurface,
        contrastCurve: new ContrastCurve(1.5, 3, 4.5, 7)
    );

    public static readonly DynamicColor OutlineVariant = DynamicColor.FromPalette(
        "outline_variant",
        s => s.NeutralVariantPalette,
        s => s.IsDark ? 30 : 80,
        background: HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5)
    );

    public static readonly DynamicColor Shadow = DynamicColor.FromPalette(
        "shadow",
        s => s.NeutralPalette,
        s => 0
    );

    public static readonly DynamicColor Scrim = DynamicColor.FromPalette(
        "scrim",
        s => s.NeutralPalette,
        s => 0
    );

    public static readonly DynamicColor SurfaceTint = DynamicColor.FromPalette(
        "surface_tint",
        s => s.PrimaryPalette,
        s => s.IsDark ? 80 : 40,
        true
    );

    public static readonly DynamicColor Primary = DynamicColor.FromPalette(
        "primary",
        s => s.PrimaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 100 : 0;
            return s.IsDark ? 80 : 40;
        },
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
        toneDeltaPair: s => new ToneDeltaPair(
            PrimaryContainer,
            Primary,
            10,
            TonePolarity.Nearer,
            false
        )
    );

    public static readonly DynamicColor OnPrimary = DynamicColor.FromPalette(
        "on_primary",
        s => s.PrimaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 10 : 90;
            return s.IsDark ? 20 : 100;
        },
        background: s => Primary,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor PrimaryContainer = DynamicColor.FromPalette(
        "primary_container",
        s => s.PrimaryPalette,
        s =>
        {
            if (IsFidelity(s))
                return s.SourceColorHct.Tone;
            if (IsMonochrome(s))
                return s.IsDark ? 85 : 25;
            return s.IsDark ? 30 : 90;
        },
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            PrimaryContainer,
            Primary,
            10,
            TonePolarity.Nearer,
            false
        )
    );

    public static readonly DynamicColor OnPrimaryContainer = DynamicColor.FromPalette(
        "on_primary_container",
        s => s.PrimaryPalette,
        s =>
        {
            if (IsFidelity(s))
                return DynamicColor.ForegroundTone(PrimaryContainer.Tone(s), 4.5);
            if (IsMonochrome(s))
                return s.IsDark ? 0 : 100;
            return s.IsDark ? 90 : 30;
        },
        background: s => PrimaryContainer,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    public static readonly DynamicColor InversePrimary = DynamicColor.FromPalette(
        "inverse_primary",
        s => s.PrimaryPalette,
        s => s.IsDark ? 40 : 80,
        background: s => InverseSurface,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 7)
    );

    public static readonly DynamicColor Secondary = DynamicColor.FromPalette(
        "secondary",
        s => s.SecondaryPalette,
        s => s.IsDark ? 80 : 40,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
        toneDeltaPair: s => new ToneDeltaPair(
            SecondaryContainer,
            Secondary,
            10,
            TonePolarity.Nearer,
            false
        )
    );

    public static readonly DynamicColor OnSecondary = DynamicColor.FromPalette(
        "on_secondary",
        s => s.SecondaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 10 : 100;
            else
                return s.IsDark ? 20 : 100;
        },
        background: s => Secondary,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor SecondaryContainer = DynamicColor.FromPalette(
        "secondary_container",
        s => s.SecondaryPalette,
        s =>
        {
            var initialTone = s.IsDark ? 30.0 : 90.0;
            if (IsMonochrome(s))
                return s.IsDark ? 30 : 85;
            if (!IsFidelity(s))
                return initialTone;
            return FindDesiredChromaByTone(
                s.SecondaryPalette.Hue,
                s.SecondaryPalette.Chroma,
                initialTone,
                !s.IsDark
            );
        },
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            SecondaryContainer,
            Secondary,
            10,
            TonePolarity.Nearer,
            false
        )
    );

    public static readonly DynamicColor OnSecondaryContainer = DynamicColor.FromPalette(
        "on_secondary_container",
        s => s.SecondaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 90 : 10;
            if (!IsFidelity(s))
                return s.IsDark ? 90 : 30;
            return DynamicColor.ForegroundTone(SecondaryContainer.Tone(s), 4.5);
        },
        background: s => SecondaryContainer,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    public static readonly DynamicColor Tertiary = DynamicColor.FromPalette(
        "tertiary",
        s => s.TertiaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 90 : 25;
            return s.IsDark ? 80 : 40;
        },
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
        toneDeltaPair: s => new ToneDeltaPair(
            TertiaryContainer,
            Tertiary,
            10,
            TonePolarity.Nearer,
            false
        )
    );

    public static readonly DynamicColor OnTertiary = DynamicColor.FromPalette(
        "on_tertiary",
        s => s.TertiaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 10 : 90;
            return s.IsDark ? 20 : 100;
        },
        background: s => Tertiary,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor TertiaryContainer = DynamicColor.FromPalette(
        "tertiary_container",
        s => s.TertiaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 60 : 49;
            if (!IsFidelity(s))
                return s.IsDark ? 30 : 90;
            var proposedHct = s.TertiaryPalette.GetHct(s.SourceColorHct.Tone);
            return DislikeAnalyzer.FixIfDisliked(proposedHct).Tone;
        },
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            TertiaryContainer,
            Tertiary,
            10,
            TonePolarity.Nearer,
            false
        )
    );

    public static readonly DynamicColor OnTertiaryContainer = DynamicColor.FromPalette(
        "on_tertiary_container",
        s => s.TertiaryPalette,
        s =>
        {
            if (IsMonochrome(s))
                return s.IsDark ? 0 : 100;
            if (!IsFidelity(s))
                return s.IsDark ? 90 : 30;
            return DynamicColor.ForegroundTone(TertiaryContainer.Tone(s), 4.5);
        },
        background: s => TertiaryContainer,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    public static readonly DynamicColor Error = DynamicColor.FromPalette(
        "error",
        s => s.ErrorPalette,
        s => s.IsDark ? 80 : 40,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
        toneDeltaPair: s => new ToneDeltaPair(ErrorContainer, Error, 10, TonePolarity.Nearer, false)
    );

    public static readonly DynamicColor OnError = DynamicColor.FromPalette(
        "on_error",
        s => s.ErrorPalette,
        s => s.IsDark ? 20 : 100,
        background: s => Error,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor ErrorContainer = DynamicColor.FromPalette(
        "error_container",
        s => s.ErrorPalette,
        s => s.IsDark ? 30 : 90,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(ErrorContainer, Error, 10, TonePolarity.Nearer, false)
    );

    public static readonly DynamicColor OnErrorContainer = DynamicColor.FromPalette(
        "on_error_container",
        s => s.ErrorPalette,
        s => s.IsDark ? 90 : 30,
        background: s => ErrorContainer,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    public static readonly DynamicColor PrimaryFixed = DynamicColor.FromPalette(
        "primary_fixed",
        s => s.PrimaryPalette,
        s => IsMonochrome(s) ? 40.0 : 90.0,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            PrimaryFixed,
            PrimaryFixedDim,
            10,
            TonePolarity.Lighter,
            true
        )
    );

    public static readonly DynamicColor PrimaryFixedDim = DynamicColor.FromPalette(
        "primary_fixed_dim",
        s => s.PrimaryPalette,
        s => IsMonochrome(s) ? 30.0 : 80.0,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            PrimaryFixed,
            PrimaryFixedDim,
            10,
            TonePolarity.Lighter,
            true
        )
    );

    public static readonly DynamicColor OnPrimaryFixed = DynamicColor.FromPalette(
        "on_primary_fixed",
        s => s.PrimaryPalette,
        s => IsMonochrome(s) ? 100.0 : 10.0,
        background: s => PrimaryFixedDim,
        secondBackground: s => PrimaryFixed,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor OnPrimaryFixedVariant = DynamicColor.FromPalette(
        "on_primary_fixed_variant",
        s => s.PrimaryPalette,
        s => IsMonochrome(s) ? 90.0 : 30.0,
        background: s => PrimaryFixedDim,
        secondBackground: s => PrimaryFixed,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    public static readonly DynamicColor SecondaryFixed = DynamicColor.FromPalette(
        "secondary_fixed",
        s => s.SecondaryPalette,
        s => IsMonochrome(s) ? 80.0 : 90.0,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            SecondaryFixed,
            SecondaryFixedDim,
            10,
            TonePolarity.Lighter,
            true
        )
    );

    public static readonly DynamicColor SecondaryFixedDim = DynamicColor.FromPalette(
        "secondary_fixed_dim",
        s => s.SecondaryPalette,
        s => IsMonochrome(s) ? 70.0 : 80.0,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            SecondaryFixed,
            SecondaryFixedDim,
            10,
            TonePolarity.Lighter,
            true
        )
    );

    public static readonly DynamicColor OnSecondaryFixed = DynamicColor.FromPalette(
        "on_secondary_fixed",
        s => s.SecondaryPalette,
        s => 10.0,
        background: s => SecondaryFixedDim,
        secondBackground: s => SecondaryFixed,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor OnSecondaryFixedVariant = DynamicColor.FromPalette(
        "on_secondary_fixed_variant",
        s => s.SecondaryPalette,
        s => IsMonochrome(s) ? 25.0 : 30.0,
        background: s => SecondaryFixedDim,
        secondBackground: s => SecondaryFixed,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    public static readonly DynamicColor TertiaryFixed = DynamicColor.FromPalette(
        "tertiary_fixed",
        s => s.TertiaryPalette,
        s => IsMonochrome(s) ? 40.0 : 90.0,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            TertiaryFixed,
            TertiaryFixedDim,
            10,
            TonePolarity.Lighter,
            true
        )
    );

    public static readonly DynamicColor TertiaryFixedDim = DynamicColor.FromPalette(
        "tertiary_fixed_dim",
        s => s.TertiaryPalette,
        s => IsMonochrome(s) ? 30.0 : 80.0,
        true,
        HighestSurface,
        contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
        toneDeltaPair: s => new ToneDeltaPair(
            TertiaryFixed,
            TertiaryFixedDim,
            10,
            TonePolarity.Lighter,
            true
        )
    );

    public static readonly DynamicColor OnTertiaryFixed = DynamicColor.FromPalette(
        "on_tertiary_fixed",
        s => s.TertiaryPalette,
        s => IsMonochrome(s) ? 100.0 : 10.0,
        background: s => TertiaryFixedDim,
        secondBackground: s => TertiaryFixed,
        contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
    );

    public static readonly DynamicColor OnTertiaryFixedVariant = DynamicColor.FromPalette(
        "on_tertiary_fixed_variant",
        s => s.TertiaryPalette,
        s => IsMonochrome(s) ? 90.0 : 30.0,
        background: s => TertiaryFixedDim,
        secondBackground: s => TertiaryFixed,
        contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
    );

    private static double FindDesiredChromaByTone(
        double hue,
        double chroma,
        double tone,
        bool byDecreasingTone
    )
    {
        var answer = tone;

        var closestToChroma = Hct.From(hue, chroma, tone);
        if (closestToChroma.Chroma < chroma)
        {
            var chromaPeak = closestToChroma.Chroma;
            while (closestToChroma.Chroma < chroma)
            {
                answer += byDecreasingTone ? -1.0 : 1.0;
                var potentialSolution = Hct.From(hue, chroma, answer);
                if (chromaPeak > potentialSolution.Chroma)
                    break;
                if (Math.Abs(potentialSolution.Chroma - chroma) < 0.4)
                    break;

                var potentialDelta = Math.Abs(potentialSolution.Chroma - chroma);
                var currentDelta = Math.Abs(closestToChroma.Chroma - chroma);
                if (potentialDelta < currentDelta)
                    closestToChroma = potentialSolution;
                chromaPeak = Math.Max(chromaPeak, potentialSolution.Chroma);
            }
        }

        return answer;
    }
}