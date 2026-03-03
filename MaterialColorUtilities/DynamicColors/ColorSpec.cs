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

using HCT;
using Palettes;

/// <summary>
/// Interface for spec-versioned dynamic color behaviors.
/// </summary>
public interface ColorSpec
{
    public enum SpecVersion
    {
        Spec2021,
        Spec2025,
        Spec2026
    }

    DynamicColor PrimaryPaletteKeyColor { get; }
    DynamicColor SecondaryPaletteKeyColor { get; }
    DynamicColor TertiaryPaletteKeyColor { get; }
    DynamicColor NeutralPaletteKeyColor { get; }
    DynamicColor NeutralVariantPaletteKeyColor { get; }
    DynamicColor ErrorPaletteKeyColor { get; }

    DynamicColor Background { get; }
    DynamicColor OnBackground { get; }
    DynamicColor Surface { get; }
    DynamicColor SurfaceDim { get; }
    DynamicColor SurfaceBright { get; }
    DynamicColor SurfaceContainerLowest { get; }
    DynamicColor SurfaceContainerLow { get; }
    DynamicColor SurfaceContainer { get; }
    DynamicColor SurfaceContainerHigh { get; }
    DynamicColor SurfaceContainerHighest { get; }
    DynamicColor OnSurface { get; }
    DynamicColor SurfaceVariant { get; }
    DynamicColor OnSurfaceVariant { get; }
    DynamicColor InverseSurface { get; }
    DynamicColor InverseOnSurface { get; }
    DynamicColor Outline { get; }
    DynamicColor OutlineVariant { get; }
    DynamicColor Shadow { get; }
    DynamicColor Scrim { get; }
    DynamicColor SurfaceTint { get; }

    DynamicColor Primary { get; }
    DynamicColor? PrimaryDim { get; }
    DynamicColor OnPrimary { get; }
    DynamicColor PrimaryContainer { get; }
    DynamicColor OnPrimaryContainer { get; }
    DynamicColor InversePrimary { get; }

    DynamicColor Secondary { get; }
    DynamicColor? SecondaryDim { get; }
    DynamicColor OnSecondary { get; }
    DynamicColor SecondaryContainer { get; }
    DynamicColor OnSecondaryContainer { get; }

    DynamicColor Tertiary { get; }
    DynamicColor? TertiaryDim { get; }
    DynamicColor OnTertiary { get; }
    DynamicColor TertiaryContainer { get; }
    DynamicColor OnTertiaryContainer { get; }

    DynamicColor Error { get; }
    DynamicColor? ErrorDim { get; }
    DynamicColor OnError { get; }
    DynamicColor ErrorContainer { get; }
    DynamicColor OnErrorContainer { get; }

    DynamicColor PrimaryFixed { get; }
    DynamicColor PrimaryFixedDim { get; }
    DynamicColor OnPrimaryFixed { get; }
    DynamicColor OnPrimaryFixedVariant { get; }
    DynamicColor SecondaryFixed { get; }
    DynamicColor SecondaryFixedDim { get; }
    DynamicColor OnSecondaryFixed { get; }
    DynamicColor OnSecondaryFixedVariant { get; }
    DynamicColor TertiaryFixed { get; }
    DynamicColor TertiaryFixedDim { get; }
    DynamicColor OnTertiaryFixed { get; }
    DynamicColor OnTertiaryFixedVariant { get; }

    DynamicColor HighestSurface(DynamicScheme scheme);

    Hct GetHct(DynamicScheme scheme, DynamicColor color);
    double GetTone(DynamicScheme scheme, DynamicColor color);

    TonalPalette GetPrimaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    );

    TonalPalette GetSecondaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    );

    TonalPalette GetTertiaryPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    );

    TonalPalette GetNeutralPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    );

    TonalPalette GetNeutralVariantPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    );

    TonalPalette? GetErrorPalette(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        DynamicScheme.Platform platform,
        double contrastLevel
    );
}