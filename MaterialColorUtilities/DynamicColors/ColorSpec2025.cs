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

using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using ContrastUtils = MaterialColorUtilities.Contrast.Contrast;

namespace MaterialColorUtilities.DynamicColors;

/// <summary>
/// Spec 2025 dynamic color behavior.
/// </summary>
public class ColorSpec2025 : ColorSpec2021
{
    private DynamicColor PhoneSurfaceBackground(DynamicScheme scheme)
    {
        return scheme.IsDark ? SurfaceBright : SurfaceDim;
    }

    private static double SharedOnSurfaceFamilyChromaMultiplier(DynamicScheme scheme)
    {
        if (scheme.SchemePlatform != DynamicScheme.Platform.Phone)
            return 1.0;

        return scheme.Variant switch
        {
            Variant.Neutral => 2.2,
            Variant.TonalSpot => 1.7,
            Variant.Expressive => Hct.IsYellow(scheme.NeutralPalette.Hue)
                ? scheme.IsDark ? 3.0 : 2.3
                : 1.6,
            _ => 1.0
        };
    }

    public override DynamicColor Background
    {
        get
        {
            var color2025 = Surface.With("background");
            return base.Background.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnBackground
    {
        get
        {
            var color2025 = OnSurface.With(
                "on_background",
                tone: s => s.SchemePlatform == DynamicScheme.Platform.Watch ? 100.0 : OnSurface.GetTone(s)
            );

            return base.OnBackground.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor Surface
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "surface",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                    {
                        if (s.IsDark)
                            return 4.0;
                        if (Hct.IsYellow(s.NeutralPalette.Hue))
                            return 99.0;
                        if (s.Variant == Variant.Vibrant)
                            return 97.0;
                        return 98.0;
                    }

                    return 0.0;
                },
                true
            );

            return base.Surface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceDim
    {
        get
        {
            var color2025 = new DynamicColor(
                "surface_dim",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.IsDark)
                        return 4.0;
                    if (Hct.IsYellow(s.NeutralPalette.Hue))
                        return 90.0;
                    if (s.Variant == Variant.Vibrant)
                        return 85.0;
                    return 87.0;
                },
                true,
                s =>
                {
                    if (s.IsDark)
                        return 1.0;
                    return s.Variant switch
                    {
                        Variant.Neutral => 2.5,
                        Variant.TonalSpot => 1.7,
                        Variant.Expressive => Hct.IsYellow(s.NeutralPalette.Hue) ? 2.7 : 1.75,
                        Variant.Vibrant => 1.36,
                        _ => 1.0
                    };
                },
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceBright
    {
        get
        {
            var color2025 = new DynamicColor(
                "surface_bright",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.IsDark)
                        return 18.0;
                    if (Hct.IsYellow(s.NeutralPalette.Hue))
                        return 99.0;
                    if (s.Variant == Variant.Vibrant)
                        return 97.0;
                    return 98.0;
                },
                true,
                s =>
                {
                    if (!s.IsDark)
                        return 1.0;
                    return s.Variant switch
                    {
                        Variant.Neutral => 2.5,
                        Variant.TonalSpot => 1.7,
                        Variant.Expressive => Hct.IsYellow(s.NeutralPalette.Hue) ? 2.7 : 1.75,
                        Variant.Vibrant => 1.36,
                        _ => 1.0
                    };
                },
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceBright.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceContainerLowest
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "surface_container_lowest",
                s => s.NeutralPalette,
                s => s.IsDark ? 0.0 : 100.0,
                true
            );

            return base.SurfaceContainerLowest.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceContainerLow
    {
        get
        {
            var color2025 = new DynamicColor(
                "surface_container_low",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                    {
                        if (s.IsDark)
                            return 6.0;
                        if (Hct.IsYellow(s.NeutralPalette.Hue))
                            return 98.0;
                        if (s.Variant == Variant.Vibrant)
                            return 95.0;
                        return 96.0;
                    }

                    return 15.0;
                },
                true,
                s =>
                {
                    if (s.SchemePlatform != DynamicScheme.Platform.Phone)
                        return 1.0;
                    return s.Variant switch
                    {
                        Variant.Neutral => 1.3,
                        Variant.TonalSpot => 1.25,
                        Variant.Expressive => Hct.IsYellow(s.NeutralPalette.Hue) ? 1.3 : 1.15,
                        Variant.Vibrant => 1.08,
                        _ => 1.0
                    };
                },
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainerLow.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceContainer
    {
        get
        {
            var color2025 = new DynamicColor(
                "surface_container",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                    {
                        if (s.IsDark)
                            return 9.0;
                        if (Hct.IsYellow(s.NeutralPalette.Hue))
                            return 96.0;
                        if (s.Variant == Variant.Vibrant)
                            return 92.0;
                        return 94.0;
                    }

                    return 20.0;
                },
                true,
                s =>
                {
                    if (s.SchemePlatform != DynamicScheme.Platform.Phone)
                        return 1.0;
                    return s.Variant switch
                    {
                        Variant.Neutral => 1.6,
                        Variant.TonalSpot => 1.4,
                        Variant.Expressive => Hct.IsYellow(s.NeutralPalette.Hue) ? 1.6 : 1.3,
                        Variant.Vibrant => 1.15,
                        _ => 1.0
                    };
                },
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceContainerHigh
    {
        get
        {
            var color2025 = new DynamicColor(
                "surface_container_high",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                    {
                        if (s.IsDark)
                            return 12.0;
                        if (Hct.IsYellow(s.NeutralPalette.Hue))
                            return 94.0;
                        if (s.Variant == Variant.Vibrant)
                            return 90.0;
                        return 92.0;
                    }

                    return 25.0;
                },
                true,
                s =>
                {
                    if (s.SchemePlatform != DynamicScheme.Platform.Phone)
                        return 1.0;
                    return s.Variant switch
                    {
                        Variant.Neutral => 1.9,
                        Variant.TonalSpot => 1.5,
                        Variant.Expressive => Hct.IsYellow(s.NeutralPalette.Hue) ? 1.95 : 1.45,
                        Variant.Vibrant => 1.22,
                        _ => 1.0
                    };
                },
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainerHigh.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceContainerHighest
    {
        get
        {
            var color2025 = new DynamicColor(
                "surface_container_highest",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.IsDark)
                        return 15.0;
                    if (Hct.IsYellow(s.NeutralPalette.Hue))
                        return 92.0;
                    if (s.Variant == Variant.Vibrant)
                        return 88.0;
                    return 90.0;
                },
                true,
                s => s.Variant switch
                {
                    Variant.Neutral => 2.2,
                    Variant.TonalSpot => 1.7,
                    Variant.Expressive => Hct.IsYellow(s.NeutralPalette.Hue) ? 2.3 : 1.6,
                    Variant.Vibrant => 1.29,
                    _ => 1.0
                },
                null,
                null,
                null,
                null,
                null
            );

            return base.SurfaceContainerHighest.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnSurface
    {
        get
        {
            var color2025 = new DynamicColor(
                "on_surface",
                s => s.NeutralPalette,
                s =>
                {
                    if (s.Variant == Variant.Vibrant)
                        return TMaxC(s.NeutralPalette, 0.0, 100.0, 1.1);

                    return DynamicColor.GetInitialToneFromBackground(sc =>
                            sc.SchemePlatform == DynamicScheme.Platform.Phone
                                ? sc.IsDark ? SurfaceBright : SurfaceDim
                                : SurfaceContainerHigh
                        )
                        .Invoke(s);
                },
                false,
                SharedOnSurfaceFamilyChromaMultiplier,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh,
                null,
                s =>
                    s.IsDark && s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(11.0)
                        : GetContrastCurve(9.0),
                null,
                null
            );

            return base.OnSurface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceVariant
    {
        get
        {
            var color2025 = SurfaceContainerHighest.With("surface_variant");
            return base.SurfaceVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnSurfaceVariant
    {
        get
        {
            var color2025 = new DynamicColor(
                "on_surface_variant",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh
                ),
                false,
                SharedOnSurfaceFamilyChromaMultiplier,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh,
                null,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                        return s.IsDark ? GetContrastCurve(6.0) : GetContrastCurve(4.5);
                    return GetContrastCurve(7.0);
                },
                null,
                null
            );

            return base.OnSurfaceVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor InverseSurface
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "inverse_surface",
                s => s.NeutralPalette,
                s => s.IsDark ? 98.0 : 4.0,
                true
            );

            return base.InverseSurface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor InverseOnSurface
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "inverse_on_surface",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(_ => InverseSurface),
                background: _ => InverseSurface,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.InverseOnSurface.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor Outline
    {
        get
        {
            var color2025 = new DynamicColor(
                "outline",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh
                ),
                false,
                SharedOnSurfaceFamilyChromaMultiplier,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh,
                null,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(3.0)
                        : GetContrastCurve(4.5),
                null,
                null
            );

            return base.Outline.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OutlineVariant
    {
        get
        {
            var color2025 = new DynamicColor(
                "outline_variant",
                s => s.NeutralPalette,
                DynamicColor.GetInitialToneFromBackground(s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh
                ),
                false,
                SharedOnSurfaceFamilyChromaMultiplier,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh,
                null,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(1.5)
                        : GetContrastCurve(3.0),
                null,
                null
            );

            return base.OutlineVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SurfaceTint
    {
        get
        {
            var color2025 = Primary.With("surface_tint");
            return base.SurfaceTint.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor Primary
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "primary",
                s => s.PrimaryPalette,
                s =>
                {
                    if (s.Variant == Variant.Neutral)
                    {
                        if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                            return s.IsDark ? 80.0 : 40.0;
                        return 90.0;
                    }

                    if (s.Variant == Variant.TonalSpot)
                    {
                        if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                            return s.IsDark ? 80.0 : TMaxC(s.PrimaryPalette);
                        return TMaxC(s.PrimaryPalette, 0.0, 90.0);
                    }

                    if (s.Variant == Variant.Expressive)
                    {
                        if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                            return TMaxC(
                                s.PrimaryPalette,
                                0.0,
                                Hct.IsYellow(s.PrimaryPalette.Hue)
                                    ? 25.0
                                    : Hct.IsCyan(s.PrimaryPalette.Hue)
                                        ? 88.0
                                        : 98.0
                            );

                        return TMaxC(s.PrimaryPalette);
                    }

                    if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                        return TMaxC(s.PrimaryPalette, 0.0, Hct.IsCyan(s.PrimaryPalette.Hue) ? 88.0 : 98.0);
                    return TMaxC(s.PrimaryPalette);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? s.IsDark ? SurfaceBright : SurfaceDim
                        : SurfaceContainerHigh,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(4.5)
                        : GetContrastCurve(7.0),
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? new ToneDeltaPair(
                            PrimaryContainer,
                            Primary,
                            5.0,
                            TonePolarity.RelativeLighter,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.Primary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor PrimaryDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "primary_dim",
                s => s.PrimaryPalette,
                s => s.Variant switch
                {
                    Variant.Neutral => 85.0,
                    Variant.TonalSpot => TMaxC(s.PrimaryPalette, 0.0, 90.0),
                    _ => TMaxC(s.PrimaryPalette)
                },
                true,
                _ => SurfaceContainerHigh,
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        PrimaryDim,
                        Primary,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Farther
                    )
            );
        }
    }

    public override DynamicColor OnPrimary
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_primary",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? Primary : PrimaryDim
                ),
                background: s => s.SchemePlatform == DynamicScheme.Platform.Phone ? Primary : PrimaryDim,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(6.0)
                        : GetContrastCurve(7.0)
            );

            return base.OnPrimary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor PrimaryContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "primary_container",
                s => s.PrimaryPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Watch)
                        return 30.0;

                    if (s.Variant == Variant.Neutral)
                        return s.IsDark ? 30.0 : 90.0;

                    if (s.Variant == Variant.TonalSpot)
                        return s.IsDark ? TMinC(s.PrimaryPalette, 35.0, 93.0) : TMaxC(s.PrimaryPalette, 0.0, 90.0);

                    if (s.Variant == Variant.Expressive)
                    {
                        if (s.IsDark)
                            return TMaxC(s.PrimaryPalette, 30.0, 93.0);

                        return TMaxC(
                            s.PrimaryPalette,
                            78.0,
                            Hct.IsCyan(s.PrimaryPalette.Hue) ? 88.0 : 90.0
                        );
                    }

                    if (s.IsDark)
                        return TMinC(s.PrimaryPalette, 66.0, 93.0);

                    return TMaxC(
                        s.PrimaryPalette,
                        66.0,
                        Hct.IsCyan(s.PrimaryPalette.Hue) ? 88.0 : 93.0
                    );
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? PhoneSurfaceBackground(s) : null,
                contrastCurve: s =>
                s.SchemePlatform == DynamicScheme.Platform.Phone && s.ContrastLevel > 0
                    ? GetContrastCurve(1.5)
                    : null,
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Watch
                        ? new ToneDeltaPair(
                            PrimaryContainer,
                            PrimaryDim,
                            10.0,
                            TonePolarity.Darker,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.PrimaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnPrimaryContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_primary_container",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => PrimaryContainer),
                background: _ => PrimaryContainer,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(6.0)
                        : GetContrastCurve(7.0)
            );

            return base.OnPrimaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor InversePrimary
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "inverse_primary",
                s => s.PrimaryPalette,
                s => TMaxC(s.PrimaryPalette),
                background: _ => InverseSurface,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(6.0)
                        : GetContrastCurve(7.0)
            );

            return base.InversePrimary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor Secondary
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "secondary",
                s => s.SecondaryPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Watch)
                        return s.Variant == Variant.Neutral ? 90.0 : TMaxC(s.SecondaryPalette, 0.0, 90.0);

                    if (s.Variant == Variant.Neutral)
                        return s.IsDark ? TMinC(s.SecondaryPalette, 0.0, 98.0) : TMaxC(s.SecondaryPalette);

                    if (s.Variant == Variant.Vibrant)
                        return TMaxC(s.SecondaryPalette, 0.0, s.IsDark ? 90.0 : 98.0);

                    return s.IsDark ? 80.0 : TMaxC(s.SecondaryPalette);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? PhoneSurfaceBackground(s)
                        : SurfaceContainerHigh,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(4.5)
                        : GetContrastCurve(7.0),
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? new ToneDeltaPair(
                            SecondaryContainer,
                            Secondary,
                            5.0,
                            TonePolarity.RelativeLighter,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.Secondary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SecondaryDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "secondary_dim",
                s => s.SecondaryPalette,
                s => s.Variant == Variant.Neutral ? 85.0 : TMaxC(s.SecondaryPalette, 0.0, 90.0),
                true,
                _ => SurfaceContainerHigh,
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        SecondaryDim!,
                        Secondary,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Farther
                    )
            );
        }
    }

    public override DynamicColor OnSecondary
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_secondary",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? Secondary : SecondaryDim
                ),
                background: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? Secondary : SecondaryDim,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(6.0)
                        : GetContrastCurve(7.0)
            );

            return base.OnSecondary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SecondaryContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "secondary_container",
                s => s.SecondaryPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Watch)
                        return 30.0;

                    if (s.Variant == Variant.Vibrant)
                        return s.IsDark
                            ? TMinC(s.SecondaryPalette, 30.0, 40.0)
                            : TMaxC(s.SecondaryPalette, 84.0, 90.0);

                    if (s.Variant == Variant.Expressive)
                        return s.IsDark ? 15.0 : TMaxC(s.SecondaryPalette, 90.0, 95.0);

                    return s.IsDark ? 25.0 : 90.0;
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? PhoneSurfaceBackground(s) : null,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone && s.ContrastLevel > 0
                        ? GetContrastCurve(1.5)
                        : null,
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Watch
                        ? new ToneDeltaPair(
                            SecondaryContainer,
                            SecondaryDim!,
                            10.0,
                            TonePolarity.Darker,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.SecondaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnSecondaryContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_secondary_container",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => SecondaryContainer),
                background: _ => SecondaryContainer,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(6.0)
                        : GetContrastCurve(7.0)
            );

            return base.OnSecondaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor Tertiary
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "tertiary",
                s => s.TertiaryPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Watch)
                        return s.Variant == Variant.TonalSpot
                            ? TMaxC(s.TertiaryPalette, 0.0, 90.0)
                            : TMaxC(s.TertiaryPalette);

                    if (s.Variant == Variant.Expressive || s.Variant == Variant.Vibrant)
                        return TMaxC(
                            s.TertiaryPalette,
                            0.0,
                            Hct.IsCyan(s.TertiaryPalette.Hue)
                                ? 88.0
                                : s.IsDark
                                    ? 98.0
                                    : 100.0
                        );

                    return s.IsDark ? TMaxC(s.TertiaryPalette, 0.0, 98.0) : TMaxC(s.TertiaryPalette);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? PhoneSurfaceBackground(s)
                        : SurfaceContainerHigh,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(4.5)
                        : GetContrastCurve(7.0),
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? new ToneDeltaPair(
                            TertiaryContainer,
                            Tertiary,
                            5.0,
                            TonePolarity.RelativeLighter,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.Tertiary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor TertiaryDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "tertiary_dim",
                s => s.TertiaryPalette,
                s => s.Variant == Variant.TonalSpot
                    ? TMaxC(s.TertiaryPalette, 0.0, 90.0)
                    : TMaxC(s.TertiaryPalette),
                true,
                _ => SurfaceContainerHigh,
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        TertiaryDim!,
                        Tertiary,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Farther
                    )
            );
        }
    }

    public override DynamicColor OnTertiary
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_tertiary",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? Tertiary : TertiaryDim
                ),
                background: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? Tertiary : TertiaryDim,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(6.0)
                        : GetContrastCurve(7.0)
            );

            return base.OnTertiary.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor TertiaryContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "tertiary_container",
                s => s.TertiaryPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Watch)
                        return s.Variant == Variant.TonalSpot
                            ? TMaxC(s.TertiaryPalette, 0.0, 90.0)
                            : TMaxC(s.TertiaryPalette);

                    if (s.Variant == Variant.Neutral)
                        return s.IsDark
                            ? TMaxC(s.TertiaryPalette, 0.0, 93.0)
                            : TMaxC(s.TertiaryPalette, 0.0, 96.0);

                    if (s.Variant == Variant.TonalSpot)
                        return TMaxC(s.TertiaryPalette, 0.0, s.IsDark ? 93.0 : 100.0);

                    if (s.Variant == Variant.Expressive)
                        return TMaxC(
                            s.TertiaryPalette,
                            75.0,
                            Hct.IsCyan(s.TertiaryPalette.Hue)
                                ? 88.0
                                : s.IsDark
                                    ? 93.0
                                    : 100.0
                        );

                    return s.IsDark
                        ? TMaxC(s.TertiaryPalette, 0.0, 93.0)
                        : TMaxC(s.TertiaryPalette, 72.0, 100.0);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? PhoneSurfaceBackground(s) : null,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone && s.ContrastLevel > 0
                        ? GetContrastCurve(1.5)
                        : null,
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Watch
                        ? new ToneDeltaPair(
                            TertiaryContainer,
                            TertiaryDim!,
                            10.0,
                            TonePolarity.Darker,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.TertiaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnTertiaryContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_tertiary_container",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => TertiaryContainer),
                background: _ => TertiaryContainer,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(6.0)
                        : GetContrastCurve(7.0)
            );

            return base.OnTertiaryContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor Error
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "error",
                s => s.ErrorPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Phone)
                        return s.IsDark ? TMinC(s.ErrorPalette, 0.0, 98.0) : TMaxC(s.ErrorPalette);

                    return TMinC(s.ErrorPalette);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? PhoneSurfaceBackground(s)
                        : SurfaceContainerHigh,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(4.5)
                        : GetContrastCurve(7.0),
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? new ToneDeltaPair(
                            ErrorContainer,
                            Error,
                            5.0,
                            TonePolarity.RelativeLighter,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.Error.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor ErrorDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "error_dim",
                s => s.ErrorPalette,
                s => TMinC(s.ErrorPalette),
                true,
                _ => SurfaceContainerHigh,
                contrastCurve: _ => GetContrastCurve(4.5),
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        ErrorDim!,
                        Error,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Farther
                    )
            );
        }
    }

    public override DynamicColor OnError
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_error",
                s => s.ErrorPalette,
                DynamicColor.GetInitialToneFromBackground(s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? Error : ErrorDim
                ),
                background: s => s.SchemePlatform == DynamicScheme.Platform.Phone ? Error : ErrorDim,
                contrastCurve: s =>
                s.SchemePlatform == DynamicScheme.Platform.Phone
                    ? GetContrastCurve(6.0)
                    : GetContrastCurve(7.0)
            );

            return base.OnError.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor ErrorContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "error_container",
                s => s.ErrorPalette,
                s =>
                {
                    if (s.SchemePlatform == DynamicScheme.Platform.Watch)
                        return 30.0;

                    return s.IsDark
                        ? TMinC(s.ErrorPalette, 30.0, 93.0)
                        : TMaxC(s.ErrorPalette, 0.0, 90.0);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? PhoneSurfaceBackground(s) : null,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone && s.ContrastLevel > 0
                        ? GetContrastCurve(1.5)
                        : null,
                toneDeltaPair: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Watch
                        ? new ToneDeltaPair(
                            ErrorContainer,
                            ErrorDim!,
                            10.0,
                            TonePolarity.Darker,
                            ToneDeltaPair.DeltaConstraint.Farther
                        )
                        : null
            );

            return base.ErrorContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnErrorContainer
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_error_container",
                s => s.ErrorPalette,
                DynamicColor.GetInitialToneFromBackground(_ => ErrorContainer),
                background: _ => ErrorContainer,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone
                        ? GetContrastCurve(4.5)
                        : GetContrastCurve(7.0)
            );

            return base.OnErrorContainer.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor PrimaryFixed
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "primary_fixed",
                s => s.PrimaryPalette,
                s =>
                {
                    var temp = DynamicScheme.From(s, false, 0.0);
                    return PrimaryContainer.GetTone(temp);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? PhoneSurfaceBackground(s) : null,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone && s.ContrastLevel > 0
                        ? GetContrastCurve(1.5)
                        : null
            );

            return base.PrimaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor PrimaryFixedDim
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "primary_fixed_dim",
                s => s.PrimaryPalette,
                s => PrimaryFixed.GetTone(s),
                true,
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        PrimaryFixedDim,
                        PrimaryFixed,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Exact
                    )
            );

            return base.PrimaryFixedDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnPrimaryFixed
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_primary_fixed",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => PrimaryFixedDim),
                background: _ => PrimaryFixedDim,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.OnPrimaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnPrimaryFixedVariant
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_primary_fixed_variant",
                s => s.PrimaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => PrimaryFixedDim),
                background: _ => PrimaryFixedDim,
                contrastCurve: _ => GetContrastCurve(4.5)
            );

            return base.OnPrimaryFixedVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SecondaryFixed
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "secondary_fixed",
                s => s.SecondaryPalette,
                s =>
                {
                    var temp = DynamicScheme.From(s, false, 0.0);
                    return SecondaryContainer.GetTone(temp);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? PhoneSurfaceBackground(s) : null,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone && s.ContrastLevel > 0
                        ? GetContrastCurve(1.5)
                        : null
            );

            return base.SecondaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor SecondaryFixedDim
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "secondary_fixed_dim",
                s => s.SecondaryPalette,
                s => SecondaryFixed.GetTone(s),
                true,
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        SecondaryFixedDim,
                        SecondaryFixed,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Exact
                    )
            );

            return base.SecondaryFixedDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnSecondaryFixed
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_secondary_fixed",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => SecondaryFixedDim),
                background: _ => SecondaryFixedDim,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.OnSecondaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnSecondaryFixedVariant
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_secondary_fixed_variant",
                s => s.SecondaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => SecondaryFixedDim),
                background: _ => SecondaryFixedDim,
                contrastCurve: _ => GetContrastCurve(4.5)
            );

            return base.OnSecondaryFixedVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor TertiaryFixed
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "tertiary_fixed",
                s => s.TertiaryPalette,
                s =>
                {
                    var temp = DynamicScheme.From(s, false, 0.0);
                    return TertiaryContainer.GetTone(temp);
                },
                true,
                s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone ? PhoneSurfaceBackground(s) : null,
                contrastCurve: s =>
                    s.SchemePlatform == DynamicScheme.Platform.Phone && s.ContrastLevel > 0
                        ? GetContrastCurve(1.5)
                        : null
            );

            return base.TertiaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor TertiaryFixedDim
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "tertiary_fixed_dim",
                s => s.TertiaryPalette,
                s => TertiaryFixed.GetTone(s),
                true,
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        TertiaryFixedDim,
                        TertiaryFixed,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Exact
                    )
            );

            return base.TertiaryFixedDim.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnTertiaryFixed
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_tertiary_fixed",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => TertiaryFixedDim),
                background: _ => TertiaryFixedDim,
                contrastCurve: _ => GetContrastCurve(7.0)
            );

            return base.OnTertiaryFixed.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor OnTertiaryFixedVariant
    {
        get
        {
            var color2025 = DynamicColor.FromPalette(
                "on_tertiary_fixed_variant",
                s => s.TertiaryPalette,
                DynamicColor.GetInitialToneFromBackground(_ => TertiaryFixedDim),
                background: _ => TertiaryFixedDim,
                contrastCurve: _ => GetContrastCurve(4.5)
            );

            return base.OnTertiaryFixedVariant.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override Hct GetHct(DynamicScheme scheme, DynamicColor color)
    {
        var palette = color.Palette(scheme);
        var tone = GetTone(scheme, color);
        var chromaMultiplier = color.ChromaMultiplier?.Invoke(scheme) ?? 1.0;
        return Hct.From(palette.Hue, palette.Chroma * chromaMultiplier, tone);
    }

    public override double GetTone(DynamicScheme scheme, DynamicColor color)
    {
        var toneDeltaPair = color.ToneDeltaPair?.Invoke(scheme);

        // Case 0: tone delta pair.
        if (toneDeltaPair != null)
        {
            var roleA = toneDeltaPair.RoleA;
            var roleB = toneDeltaPair.RoleB;
            var polarity = toneDeltaPair.Polarity;
            var constraint = toneDeltaPair.Constraint;

            var absoluteDelta =
                polarity == TonePolarity.Darker
                || (polarity == TonePolarity.RelativeLighter && scheme.IsDark)
                || (polarity == TonePolarity.RelativeDarker && !scheme.IsDark)
                    ? -toneDeltaPair.Delta
                    : toneDeltaPair.Delta;

            var amRoleA = color.Name == roleA.Name;
            var selfRole = amRoleA ? roleA : roleB;
            var referenceRole = amRoleA ? roleB : roleA;

            var selfTone = selfRole.Tone(scheme);
            var referenceTone = referenceRole.GetTone(scheme);
            var relativeDelta = absoluteDelta * (amRoleA ? 1.0 : -1.0);

            switch (constraint)
            {
                case ToneDeltaPair.DeltaConstraint.Exact:
                    selfTone = Math.Clamp(referenceTone + relativeDelta, 0.0, 100.0);
                    break;
                case ToneDeltaPair.DeltaConstraint.Nearer:
                    selfTone = relativeDelta > 0
                        ? Math.Clamp(selfTone, referenceTone, referenceTone + relativeDelta)
                        : Math.Clamp(selfTone, referenceTone + relativeDelta, referenceTone);
                    selfTone = Math.Clamp(selfTone, 0.0, 100.0);
                    break;
                case ToneDeltaPair.DeltaConstraint.Farther:
                    selfTone = relativeDelta > 0
                        ? Math.Clamp(selfTone, referenceTone + relativeDelta, 100.0)
                        : Math.Clamp(selfTone, 0.0, referenceTone + relativeDelta);
                    break;
            }

            var background = color.Background?.Invoke(scheme);
            var contrastCurve = color.ContrastCurve?.Invoke(scheme);
            if (background != null && contrastCurve != null)
            {
                var bgTone = background.GetTone(scheme);
                var selfContrast = contrastCurve.Get(scheme.ContrastLevel);
                selfTone =
                    ContrastUtils.RatioOfTones(bgTone, selfTone) >= selfContrast && scheme.ContrastLevel >= 0
                        ? selfTone
                        : DynamicColor.ForegroundTone(bgTone, selfContrast);
            }

            if (color.IsBackground && !color.Name.EndsWith("_fixed_dim"))
                selfTone = selfTone >= 57
                    ? Math.Clamp(selfTone, 65.0, 100.0)
                    : Math.Clamp(selfTone, 0.0, 49.0);

            return selfTone;
        }

        var answer = color.Tone(scheme);
        var bg = color.Background?.Invoke(scheme);
        var curve = color.ContrastCurve?.Invoke(scheme);
        if (bg == null || curve == null)
            return answer;

        var bgToneSingle = bg.GetTone(scheme);
        var desiredRatio = curve.Get(scheme.ContrastLevel);

        answer = ContrastUtils.RatioOfTones(bgToneSingle, answer) >= desiredRatio && scheme.ContrastLevel >= 0
            ? answer
            : DynamicColor.ForegroundTone(bgToneSingle, desiredRatio);

        if (color.IsBackground && !color.Name.EndsWith("_fixed_dim"))
            answer = answer >= 57
                ? Math.Clamp(answer, 65.0, 100.0)
                : Math.Clamp(answer, 0.0, 49.0);

        var secondBackground = color.SecondBackground?.Invoke(scheme);
        if (secondBackground == null)
            return answer;

        var bgTone1 = bg.GetTone(scheme);
        var bgTone2 = secondBackground.GetTone(scheme);
        var upper = Math.Max(bgTone1, bgTone2);
        var lower = Math.Min(bgTone1, bgTone2);

        if (
            ContrastUtils.RatioOfTones(upper, answer) >= desiredRatio
            && ContrastUtils.RatioOfTones(lower, answer) >= desiredRatio
        )
            return answer;

        var lightOption = ContrastUtils.Lighter(upper, desiredRatio);
        var darkOption = ContrastUtils.Darker(lower, desiredRatio);
        var availables = new List<double>();
        if (lightOption is {})
            availables.Add(lightOption.Value);
        if (darkOption is {})
            availables.Add(darkOption.Value);

        var prefersLight =
            DynamicColor.TonePrefersLightForeground(bgTone1)
            || DynamicColor.TonePrefersLightForeground(bgTone2);

        if (prefersLight)
            return lightOption ?? 100.0;
        if (availables.Count == 1)
            return availables[0];
        return darkOption ?? 0.0;
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

        return variant switch
        {
            Variant.Neutral => new TonalPalette(
                sourceColorHct.Hue,
                platform == DynamicScheme.Platform.Phone
                    ? Hct.IsBlue(sourceColorHct.Hue) ? 12.0 : 8.0
                    : Hct.IsBlue(sourceColorHct.Hue)
                        ? 16.0
                        : 12.0
            ),
            Variant.TonalSpot => new TonalPalette(
                sourceColorHct.Hue,
                platform == DynamicScheme.Platform.Phone && isDark ? 26.0 : 32.0
            ),
            Variant.Expressive => new TonalPalette(
                sourceColorHct.Hue,
                platform == DynamicScheme.Platform.Phone
                    ? isDark ? 36.0 : 48.0
                    : 40.0
            ),
            Variant.Vibrant => new TonalPalette(
                sourceColorHct.Hue,
                platform == DynamicScheme.Platform.Phone ? 74.0 : 56.0
            ),
            _ => base.GetPrimaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel)
        };
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

        return variant switch
        {
            Variant.Neutral => new TonalPalette(
                sourceColorHct.Hue,
                platform == DynamicScheme.Platform.Phone
                    ? Hct.IsBlue(sourceColorHct.Hue) ? 6.0 : 4.0
                    : Hct.IsBlue(sourceColorHct.Hue)
                        ? 10.0
                        : 6.0
            ),
            Variant.TonalSpot => new TonalPalette(sourceColorHct.Hue, 16.0),
            Variant.Expressive => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0.0, 105.0, 140.0, 204.0, 253.0, 278.0, 300.0, 333.0, 360.0],
                    [-160.0, 155.0, -100.0, 96.0, -96.0, -156.0, -165.0, -160.0]
                ),
                platform == DynamicScheme.Platform.Phone
                    ? isDark ? 16.0 : 24.0
                    : 24.0
            ),
            Variant.Vibrant => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0.0, 38.0, 105.0, 140.0, 333.0, 360.0],
                    [-14.0, 10.0, -14.0, 10.0, -14.0]
                ),
                platform == DynamicScheme.Platform.Phone ? 56.0 : 36.0
            ),
            _ => base.GetSecondaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel)
        };
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

        return variant switch
        {
            Variant.Neutral => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0.0, 38.0, 105.0, 161.0, 204.0, 278.0, 333.0, 360.0],
                    [-32.0, 26.0, 10.0, -39.0, 24.0, -15.0, -32.0]
                ),
                platform == DynamicScheme.Platform.Phone ? 20.0 : 36.0
            ),
            Variant.TonalSpot => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0.0, 20.0, 71.0, 161.0, 333.0, 360.0],
                    [-40.0, 48.0, -32.0, 40.0, -32.0]
                ),
                platform == DynamicScheme.Platform.Phone ? 28.0 : 32.0
            ),
            Variant.Expressive => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0.0, 105.0, 140.0, 204.0, 253.0, 278.0, 300.0, 333.0, 360.0],
                    [-165.0, 160.0, -105.0, 101.0, -101.0, -160.0, -170.0, -165.0]
                ),
                48.0
            ),
            Variant.Vibrant => new TonalPalette(
                DynamicScheme.GetRotatedHue(
                    sourceColorHct,
                    [0.0, 38.0, 71.0, 105.0, 140.0, 161.0, 253.0, 333.0, 360.0],
                    [-72.0, 35.0, 24.0, -24.0, 62.0, 50.0, 62.0, -72.0]
                ),
                56.0
            ),
            _ => base.GetTertiaryPalette(variant, sourceColorHct, isDark, platform, contrastLevel)
        };
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

        return variant switch
        {
            Variant.Neutral => new TonalPalette(sourceColorHct.Hue, platform == DynamicScheme.Platform.Phone ? 1.4 : 6.0),
            Variant.TonalSpot => new TonalPalette(sourceColorHct.Hue, platform == DynamicScheme.Platform.Phone ? 5.0 : 10.0),
            Variant.Expressive => new TonalPalette(
                GetExpressiveNeutralHue(sourceColorHct),
                GetExpressiveNeutralChroma(sourceColorHct, isDark, platform)
            ),
            Variant.Vibrant => new TonalPalette(
                GetVibrantNeutralHue(sourceColorHct),
                GetVibrantNeutralChroma(sourceColorHct, platform)
            ),
            _ => base.GetNeutralPalette(variant, sourceColorHct, isDark, platform, contrastLevel)
        };
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

        if (variant == Variant.Neutral)
            return new TonalPalette(sourceColorHct.Hue, (platform == DynamicScheme.Platform.Phone ? 1.4 : 6.0) * 2.2);

        if (variant == Variant.TonalSpot)
            return new TonalPalette(sourceColorHct.Hue, (platform == DynamicScheme.Platform.Phone ? 5.0 : 10.0) * 1.7);

        if (variant == Variant.Expressive)
        {
            var expressiveNeutralHue = GetExpressiveNeutralHue(sourceColorHct);
            var expressiveNeutralChroma = GetExpressiveNeutralChroma(sourceColorHct, isDark, platform);
            return new TonalPalette(
                expressiveNeutralHue,
                expressiveNeutralChroma * (Hct.IsYellow(expressiveNeutralHue) ? 1.6 : 2.3)
            );
        }

        if (variant == Variant.Vibrant)
        {
            var vibrantNeutralHue = GetVibrantNeutralHue(sourceColorHct);
            var vibrantNeutralChroma = GetVibrantNeutralChroma(sourceColorHct, platform);
            return new TonalPalette(vibrantNeutralHue, vibrantNeutralChroma * 1.29);
        }

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

        var errorHue = DynamicScheme.GetPiecewiseValue(
            sourceColorHct,
            [0.0, 3.0, 13.0, 23.0, 33.0, 43.0, 153.0, 273.0, 360.0],
            [12.0, 22.0, 32.0, 12.0, 22.0, 32.0, 22.0, 12.0]
        );

        return variant switch
        {
            Variant.Neutral => new TonalPalette(errorHue, platform == DynamicScheme.Platform.Phone ? 50.0 : 40.0),
            Variant.TonalSpot => new TonalPalette(errorHue, platform == DynamicScheme.Platform.Phone ? 60.0 : 48.0),
            Variant.Expressive => new TonalPalette(errorHue, platform == DynamicScheme.Platform.Phone ? 64.0 : 48.0),
            Variant.Vibrant => new TonalPalette(errorHue, platform == DynamicScheme.Platform.Phone ? 80.0 : 60.0),
            _ => base.GetErrorPalette(variant, sourceColorHct, isDark, platform, contrastLevel)
        };
    }

    private static double GetExpressiveNeutralHue(Hct sourceColorHct)
    {
        return DynamicScheme.GetRotatedHue(
            sourceColorHct,
            [0.0, 71.0, 124.0, 253.0, 278.0, 300.0, 360.0],
            [10.0, 0.0, 10.0, 0.0, 10.0, 0.0]
        );
    }

    private static double GetExpressiveNeutralChroma(
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform
    )
    {
        var neutralHue = GetExpressiveNeutralHue(sourceColorHct);
        if (platform == DynamicScheme.Platform.Phone)
        {
            if (isDark)
                return Hct.IsYellow(neutralHue) ? 6.0 : 14.0;
            return 18.0;
        }

        return 12.0;
    }

    private static double GetVibrantNeutralHue(Hct sourceColorHct)
    {
        return DynamicScheme.GetRotatedHue(
            sourceColorHct,
            [0.0, 38.0, 105.0, 140.0, 333.0, 360.0],
            [-14.0, 10.0, -14.0, 10.0, -14.0]
        );
    }

    private static double GetVibrantNeutralChroma(Hct sourceColorHct, DynamicScheme.Platform platform)
    {
        var neutralHue = GetVibrantNeutralHue(sourceColorHct);
        if (platform == DynamicScheme.Platform.Phone)
            return 28.0;
        return Hct.IsBlue(neutralHue) ? 28.0 : 20.0;
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
