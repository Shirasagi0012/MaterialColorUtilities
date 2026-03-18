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

using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;

namespace MaterialColorUtilities.DynamicColors;

/// <summary>
/// Spec 2026 dynamic color behavior.
/// </summary>
public sealed class ColorSpec2026 : ColorSpec2025
{
    public override DynamicColor Surface
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "surface",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 4.0 : 98.0 : 0.0,
                true
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
                s => s.Variant == Variant.Cmf ? s.IsDark ? 4.0 : 87.0 : 0.0,
                true,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 1.0 : 1.7 : 0.0,
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
                s => s.Variant == Variant.Cmf ? s.IsDark ? 18.0 : 98.0 : 0.0,
                true,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 1.7 : 1.0 : 0.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceBright.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SurfaceContainerLowest
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "surface_container_lowest",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 0.0 : 100.0 : 0.0,
                true
            );

            return base.SurfaceContainerLowest.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SurfaceContainerLow
    {
        get
        {
            var color2026 = new DynamicColor(
                "surface_container_low",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 6.0 : 96.0 : 0.0,
                true,
                s => s.Variant == Variant.Cmf ? 1.25 : 0.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainerLow.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SurfaceContainer
    {
        get
        {
            var color2026 = new DynamicColor(
                "surface_container",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 9.0 : 94.0 : 0.0,
                true,
                s => s.Variant == Variant.Cmf ? 1.4 : 0.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SurfaceContainerHigh
    {
        get
        {
            var color2026 = new DynamicColor(
                "surface_container_high",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 12.0 : 92.0 : 0.0,
                true,
                s => s.Variant == Variant.Cmf ? 1.5 : 0.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainerHigh.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SurfaceContainerHighest
    {
        get
        {
            var color2026 = new DynamicColor(
                "surface_container_highest",
                s => s.NeutralPalette,
                s => s.Variant == Variant.Cmf ? s.IsDark ? 15.0 : 90.0 : 0.0,
                true,
                s => s.Variant == Variant.Cmf ? 1.7 : 0.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainerHighest.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnSurface
    {
        get
        {
            var color2026 = new DynamicColor(
                "on_surface",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(s => HighestSurface(s)),
                false,
                s => s.Variant == Variant.Cmf ? 1.7 : 0.0,
                s => HighestSurface(s),
                null,
                s => GetContrastCurve(s.IsDark ? 11.0 : 9.0),
                null,
                null
            );

            return base.OnSurface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnSurfaceVariant
    {
        get
        {
            var color2026 = new DynamicColor(
                "on_surface_variant",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(s => HighestSurface(s)),
                false,
                s => s.Variant == Variant.Cmf ? 1.7 : 0.0,
                s => HighestSurface(s),
                null,
                s => GetContrastCurve(s.IsDark ? 6.0 : 4.5),
                null,
                null
            );

            return base.OnSurfaceVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor Outline
    {
        get
        {
            var color2026 = new DynamicColor(
                "outline",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(s => HighestSurface(s)),
                false,
                s => s.Variant == Variant.Cmf ? 1.7 : 0.0,
                s => HighestSurface(s),
                null,
                _ => GetContrastCurve(3.0),
                null,
                null
            );

            return base.Outline.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OutlineVariant
    {
        get
        {
            var color2026 = new DynamicColor(
                "outline_variant",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(s => HighestSurface(s)),
                false,
                s => s.Variant == Variant.Cmf ? 1.7 : 0.0,
                s => HighestSurface(s),
                null,
                _ => GetContrastCurve(1.5),
                null,
                null
            );

            return base.OutlineVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor InverseSurface
    {
        get
        {
            var color2026 = new DynamicColor(
                "inverse_surface",
                s => s.NeutralPalette,
                s => s.IsDark ? 98.0 : 4.0,
                true,
                s => s.Variant == Variant.Cmf ? 1.7 : 0.0,
                null,
                null,
                null,
                null,
                null
            );

            return base.InverseSurface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor InverseOnSurface
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "inverse_on_surface",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(_ => InverseSurface),
                background: _ => InverseSurface,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.InverseOnSurface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor Primary
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "primary",
                s => s.PrimaryPalette,
                s =>
                {
                    if (s.SourceColorHct.Chroma <= 12)
                        return s.IsDark ? 80.0 : 40.0;
                    return s.SourceColorHct.Tone;
                },
                true,
                s => HighestSurface(s),
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: s => s.PlatformType == DynamicScheme.Platform.Phone
                    ? new ToneDeltaPair(PrimaryContainer, Primary, 5.0, ToneDeltaPair.TonePolarity.RelativeLighter,
                        constraint: ToneDeltaPair.DeltaConstraint.Farther)
                    : null);

            return base.Primary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor PrimaryDim
    {
        get
        {
            // Remapped to primary in 2026 spec.
            var color2026 = Primary.With(name: "primary_dim");
            return base.PrimaryDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnPrimary
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_primary",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => Primary),
                background: _ => Primary,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnPrimary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor PrimaryContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "primary_container",
                s => s.PrimaryPalette,
                s =>
                {
                    if (!s.IsDark && s.SourceColorHct.Chroma <= 12)
                        return 90.0;

                    if (s.SourceColorHct.Tone > 55)
                        return Math.Clamp(s.SourceColorHct.Tone, 61.0, 90.0);

                    return Math.Clamp(s.SourceColorHct.Tone, 30.0, 49.0);
                },
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null
            );

            return base.PrimaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnPrimaryContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_primary_container",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => PrimaryContainer),
                background: _ => PrimaryContainer,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnPrimaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor PrimaryFixed
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "primary_fixed",
                s => s.PrimaryPalette,
                s =>
                {
                    var temp = DynamicScheme.From(s, false, 0.0);
                    return PrimaryContainer.GetTone(temp);
                },
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null
            );

            return base.PrimaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor PrimaryFixedDim
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "primary_fixed_dim",
                s => s.PrimaryPalette,
                s => PrimaryFixed.GetTone(s),
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null,
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        PrimaryFixedDim,
                        PrimaryFixed,
                        5.0,
                        ToneDeltaPair.TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Exact
                    )
            );

            return base.PrimaryFixedDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnPrimaryFixed
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_primary_fixed",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => PrimaryFixedDim),
                background: s => PrimaryFixed.GetTone(s) > 57 ? PrimaryFixedDim : PrimaryFixed,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.OnPrimaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnPrimaryFixedVariant
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_primary_fixed_variant",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => PrimaryFixedDim),
                background: s => PrimaryFixed.GetTone(s) > 57 ? PrimaryFixedDim : PrimaryFixed,
                contrastCurve: _ => GetContrastCurve(4.5)
            );

            return base.OnPrimaryFixedVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor Secondary
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "secondary",
                s => s.SecondaryPalette,
                s => s.IsDark ? TMinC(s.SecondaryPalette) : TMaxC(s.SecondaryPalette),
                true,
                s => HighestSurface(s),
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: s=> s.PlatformType == DynamicScheme.Platform.Phone
                    ? new ToneDeltaPair(SecondaryContainer, Secondary, 5.0, ToneDeltaPair.TonePolarity.RelativeLighter,
                        constraint: ToneDeltaPair.DeltaConstraint.Farther)
                    : null
            );

            return base.Secondary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SecondaryDim
    {
        get
        {
            // Remapped to primary in 2026 spec.
            var color2026 = Secondary.With(name: "secondary_dim");
            return base.SecondaryDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnSecondary
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_secondary",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => Secondary),
                background: _ => Secondary,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnSecondary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SecondaryContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "secondary_container",
                s => s.SecondaryPalette,
                s => s.IsDark
                    ? TMinC(s.SecondaryPalette, 20.0, 49.0)
                    : TMaxC(s.SecondaryPalette, 61.0, 90.0),
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null
            );

            return base.SecondaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnSecondaryContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_secondary_container",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => SecondaryContainer),
                background: _ => SecondaryContainer,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnSecondaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SecondaryFixed
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "secondary_fixed",
                s => s.SecondaryPalette,
                s =>
                {
                    var temp = DynamicScheme.From(s, false, 0.0);
                    return SecondaryContainer.GetTone(temp);
                },
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null
            );

            return base.SecondaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor SecondaryFixedDim
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "secondary_fixed_dim",
                s => s.SecondaryPalette,
                s => SecondaryFixed.GetTone(s),
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null,
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        SecondaryFixedDim,
                        SecondaryFixed,
                        5.0,
                        ToneDeltaPair.TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Exact
                    )
            );

            return base.SecondaryFixedDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnSecondaryFixed
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_secondary_fixed",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => SecondaryFixedDim),
                background: s => SecondaryFixed.GetTone(s) > 57 ? SecondaryFixedDim : SecondaryFixed,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.OnSecondaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnSecondaryFixedVariant
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_secondary_fixed_variant",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => SecondaryFixedDim),
                background: s => SecondaryFixed.GetTone(s) > 57 ? SecondaryFixedDim : SecondaryFixed,
                contrastCurve: _ => GetContrastCurve(4.5)
            );

            return base.OnSecondaryFixedVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor Tertiary
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "tertiary",
                s => s.TertiaryPalette,
                s => s.SourceColorHctList.Count > 1 ? s.SourceColorHctList[1].Tone : s.SourceColorHct.Tone,
                true,
                s => HighestSurface(s),
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: s=> s.PlatformType == DynamicScheme.Platform.Phone
                    ? new ToneDeltaPair(TertiaryContainer, Tertiary, 5.0, ToneDeltaPair.TonePolarity.RelativeLighter,
                        constraint: ToneDeltaPair.DeltaConstraint.Farther)
                    : null
            );

            return base.Tertiary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }
    
    public override DynamicColor TertiaryDim
    {
        get
        {
            // Remapped to primary in 2026 spec.
            var color2026 = Tertiary.With(name: "tertiary_dim");
            return base.TertiaryDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnTertiary
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_tertiary",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => Tertiary),
                background: _ => Tertiary,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnTertiary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor TertiaryContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "tertiary_container",
                s => s.TertiaryPalette,
                s =>
                {
                    var secondarySourceColorHct = s.SourceColorHctList.Count > 1
                        ? s.SourceColorHctList[1]
                        : s.SourceColorHct;

                    if (secondarySourceColorHct.Tone > 55)
                        return Math.Clamp(secondarySourceColorHct.Tone, 61.0, 90.0);
                    return Math.Clamp(secondarySourceColorHct.Tone, 20.0, 49.0);
                },
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null
            );

            return base.TertiaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnTertiaryContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_tertiary_container",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => TertiaryContainer),
                background: _ => TertiaryContainer,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnTertiaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor TertiaryFixed
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "tertiary_fixed",
                s => s.TertiaryPalette,
                s =>
                {
                    var temp = DynamicScheme.From(s, false, 0.0);
                    return TertiaryContainer.GetTone(temp);
                },
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null
            );

            return base.TertiaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor TertiaryFixedDim
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "tertiary_fixed_dim",
                s => s.TertiaryPalette,
                s => TertiaryFixed.GetTone(s),
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null,
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        TertiaryFixedDim,
                        TertiaryFixed,
                        5.0,
                        ToneDeltaPair.TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Exact
                    )
            );

            return base.TertiaryFixedDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnTertiaryFixed
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_tertiary_fixed",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => TertiaryFixedDim),
                background: s => TertiaryFixed.GetTone(s) > 57 ? TertiaryFixedDim : TertiaryFixed,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.OnTertiaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnTertiaryFixedVariant
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_tertiary_fixed_variant",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => TertiaryFixedDim),
                background: s => TertiaryFixed.GetTone(s) > 57 ? TertiaryFixedDim : TertiaryFixed,
                contrastCurve: _ => GetContrastCurve(4.5)
            );

            return base.OnTertiaryFixedVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor Error
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "error",
                s => s.ErrorPalette,
                s => TMaxC(s.ErrorPalette),
                true,
                s => HighestSurface(s),
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: s=> s.PlatformType == DynamicScheme.Platform.Phone
                    ? new ToneDeltaPair(ErrorContainer, Error, 5.0, ToneDeltaPair.TonePolarity.RelativeLighter,
                        constraint: ToneDeltaPair.DeltaConstraint.Farther)
                    : null
            );

            return base.Error.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }
    
    public override DynamicColor ErrorDim
    {
        get
        {
            // Remapped to primary in 2026 spec.
            var color2026 = Error.With(name: "error_dim");
            return base.ErrorDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnError
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_error",
                s => s.ErrorPalette,
                DynamicColor.GetInitialToneFromBackground(_ => Error),
                background: _ => Error,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnError.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor ErrorContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "error_container",
                s => s.ErrorPalette,
                s => s.IsDark ? TMinC(s.ErrorPalette) : TMaxC(s.ErrorPalette),
                true,
                s => HighestSurface(s),
                contrastCurve: s => s.ContrastLevel > 0 ? GetContrastCurve(1.5) : null
            );

            return base.ErrorContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

    public override DynamicColor OnErrorContainer
    {
        get
        {
            var color2026 = DynamicColor.FromPalette(
                "on_error_container",
                s => s.ErrorPalette,
                DynamicColor.GetInitialToneFromBackground(_ => ErrorContainer),
                background: _ => ErrorContainer,
                contrastCurve: _ => GetContrastCurve(6.0)
            );

            return base.OnErrorContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2026, color2026);
        }
    }

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

    private static double TMinC(TonalPalette palette, double lowerBound = 0, double upperBound = 100)
    {
        var answer = FindBestToneForChroma(
            palette.Hue,
            palette.Chroma,
            0,
            false
        );
        return Math.Clamp(answer, lowerBound, upperBound);
    }

    private static ContrastCurve GetContrastCurve(double defaultContrast)
    {
        return defaultContrast switch
        {
            1.5 => new ContrastCurve(1.5, 1.5, 3.0, 5.5),
            3.0 => new ContrastCurve(3.0, 3.0, 4.5, 7.0),
            4.5 => new ContrastCurve(4.5, 4.5, 7.0, 11.0),
            6.0 => new ContrastCurve(6.0, 6.0, 7.0, 11.0),
            7.0 => new ContrastCurve(7.0, 7.0, 11.0, 21.0),
            9.0 => new ContrastCurve(9.0, 9.0, 11.0, 21.0),
            11.0 => new ContrastCurve(11.0, 11.0, 21.0, 21.0),
            21.0 => new ContrastCurve(21.0, 21.0, 21.0, 21.0),
            _ => new ContrastCurve(defaultContrast, defaultContrast, 7.0, 21.0)
        };
    }
}
