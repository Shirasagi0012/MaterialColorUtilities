// Copyright 2026 Google LLC
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
/// Spec 2026 dynamic color behavior.
/// </summary>
public sealed class ColorSpec2026 : ColorSpec2025
{
    private static double FindBestToneForChroma(
        double hue,
        double chroma,
        double tone,
        bool byDecreasingTone
    )
    {
        var answer = tone;
        var bestCandidate = Hct.From(hue, chroma, answer);
        while (bestCandidate.Chroma < chroma)
        {
            if (tone is < 0 or > 100)
                break;

            tone += byDecreasingTone ? -1.0 : 1.0;
            var newCandidate = Hct.From(hue, chroma, tone);
            if (bestCandidate.Chroma < newCandidate.Chroma)
            {
                bestCandidate = newCandidate;
                answer = tone;
            }
        }

        return answer;
    }

    private static double TMaxC(TonalPalette palette, double lowerBound = 0, double upperBound = 100, double chromaMultiplier = 1)
    {
        var answer = FindBestToneForChroma(
            palette.Hue,
            palette.Chroma * chromaMultiplier,
            100,
            true
        );
        return Math.Clamp(answer, lowerBound, upperBound);
    }

    public override DynamicColor Surface
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "surface",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf
                    ? (s.IsDark ? 4.0 : 98.0)
                    : base.Surface.GetTone(s),
                isBackground: true
            );

            return base.Surface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SurfaceDim
    {
        get
        {
            var color2026 = new DynamicColor(
                "surface_dim",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf
                    ? (s.IsDark ? 4.0 : 87.0)
                    : base.SurfaceDim.GetTone(s),
                true,
                s => s.Variant == Variant.Cmf ? (s.IsDark ? 1.0 : 1.7) : 1.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SurfaceBright
    {
        get
        {
            var color2026 = new DynamicColor(
                "surface_bright",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf
                    ? (s.IsDark ? 18.0 : 98.0)
                    : base.SurfaceBright.GetTone(s),
                true,
                s => s.Variant == Variant.Cmf ? (s.IsDark ? 1.7 : 1.0) : 1.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceBright.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override TonalPalette GetPrimaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        if (variant == Variant.Cmf)
            return new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma);
        return base.GetPrimaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
    }

    public override TonalPalette GetSecondaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        if (variant == Variant.Cmf)
            return new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma * 0.5);
        return base.GetSecondaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
    }

    public override TonalPalette GetTertiaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        if (variant == Variant.Cmf)
            return new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma * 0.75);
        return base.GetTertiaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
    }

    public override TonalPalette GetNeutralPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        if (variant == Variant.Cmf)
            return new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma * 0.2);
        return base.GetNeutralPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
    }

    public override TonalPalette GetNeutralVariantPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        if (variant == Variant.Cmf)
            return new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma * 0.2);
        return base.GetNeutralVariantPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
    }

    public override TonalPalette? GetErrorPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        if (variant == Variant.Cmf)
            return new TonalPalette(23.0, Math.Max(sourceColorHct.Chroma, 50.0));
        return base.GetErrorPalette(variant, sourceColorHct, isDark, platform, contrastLevel);
    }
}
