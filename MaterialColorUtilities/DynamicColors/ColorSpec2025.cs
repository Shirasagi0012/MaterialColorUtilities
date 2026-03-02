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

/// <summary>
/// Spec 2025 dynamic color behavior.
/// </summary>
public class ColorSpec2025 : ColorSpec2021
{
    private static void ValidateExtendedColor(
        ColorSpec.SpecVersion specVersion,
        DynamicColor baseColor,
        DynamicColor extendedColor
    )
    {
        if (baseColor.Name != extendedColor.Name)
        {
            throw new ArgumentException(
                $"Attempting to extend color {baseColor.Name} with color {extendedColor.Name} of different name for spec version {specVersion}."
            );
        }

        if (baseColor.IsBackground != extendedColor.IsBackground)
        {
            throw new ArgumentException(
                $"Attempting to extend color {baseColor.Name} as a {(baseColor.IsBackground ? "background" : "foreground")} with color {extendedColor.Name} as a {(extendedColor.IsBackground ? "background" : "foreground")} for spec version {specVersion}."
            );
        }
    }

    public override DynamicColor OnBackground
    {
        get
        {
            var color2025 = new DynamicColor(
                "on_background",
                OnSurface.Palette,
                s => s.SchemePlatform == DynamicScheme.Platform.Watch ? 100.0 : OnSurface.GetTone(s),
                OnSurface.IsBackground,
                OnSurface.ChromaMultiplier,
                OnSurface.Background,
                OnSurface.SecondBackground,
                OnSurface.ContrastCurve,
                OnSurface.ToneDeltaPair,
                OnSurface.Opacity
            );

            return base.OnBackground.ExtendSpecVersion(ColorSpec.SpecVersion.Spec2025, color2025);
        }
    }

    public override DynamicColor? PrimaryDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "primary_dim",
                s => s.PrimaryPalette,
                s =>
                {
                    if (s.Variant == Variant.Neutral)
                        return 85.0;
                    return s.IsDark ? 75.0 : 85.0;
                },
                isBackground: true,
                background: _ => SurfaceContainerHigh,
                contrastCurve: new ContrastCurve(4.5, 4.5, 7, 11),
                toneDeltaPair: _ =>
                    new ToneDeltaPair(
                        PrimaryDim!,
                        Primary,
                        5.0,
                        TonePolarity.Darker,
                        ToneDeltaPair.DeltaConstraint.Farther
                    )
            );
        }
    }

    public override DynamicColor? SecondaryDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "secondary_dim",
                s => s.SecondaryPalette,
                s =>
                {
                    if (s.Variant == Variant.Neutral)
                        return 85.0;
                    return s.IsDark ? 75.0 : 85.0;
                },
                isBackground: true,
                background: _ => SurfaceContainerHigh,
                contrastCurve: new ContrastCurve(4.5, 4.5, 7, 11),
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

    public override DynamicColor? TertiaryDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "tertiary_dim",
                s => s.TertiaryPalette,
                s => s.IsDark ? 75.0 : 85.0,
                isBackground: true,
                background: _ => SurfaceContainerHigh,
                contrastCurve: new ContrastCurve(4.5, 4.5, 7, 11),
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

    public override DynamicColor? ErrorDim
    {
        get
        {
            return DynamicColor.FromPalette(
                "error_dim",
                s => s.ErrorPalette,
                s => s.IsDark ? 75.0 : 85.0,
                isBackground: true,
                background: _ => SurfaceContainerHigh,
                contrastCurve: new ContrastCurve(4.5, 4.5, 7, 11),
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
}
