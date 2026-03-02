// Copyright 2023 Google LLC
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
/// Named colors (roles) in the Material Design system.
/// </summary>
public sealed class MaterialDynamicColors
{
    private readonly ColorSpec _colorSpec = ColorSpecs.Get(ColorSpec.SpecVersion.Spec2026);

    public DynamicColor HighestSurface(DynamicScheme scheme) => _colorSpec.HighestSurface(scheme);

    public DynamicColor PrimaryPaletteKeyColor => _colorSpec.PrimaryPaletteKeyColor;
    public DynamicColor SecondaryPaletteKeyColor => _colorSpec.SecondaryPaletteKeyColor;
    public DynamicColor TertiaryPaletteKeyColor => _colorSpec.TertiaryPaletteKeyColor;
    public DynamicColor NeutralPaletteKeyColor => _colorSpec.NeutralPaletteKeyColor;
    public DynamicColor NeutralVariantPaletteKeyColor => _colorSpec.NeutralVariantPaletteKeyColor;
    public DynamicColor ErrorPaletteKeyColor => _colorSpec.ErrorPaletteKeyColor;
    public DynamicColor Background => _colorSpec.Background;
    public DynamicColor OnBackground => _colorSpec.OnBackground;
    public DynamicColor Surface => _colorSpec.Surface;
    public DynamicColor SurfaceDim => _colorSpec.SurfaceDim;
    public DynamicColor SurfaceBright => _colorSpec.SurfaceBright;
    public DynamicColor SurfaceContainerLowest => _colorSpec.SurfaceContainerLowest;
    public DynamicColor SurfaceContainerLow => _colorSpec.SurfaceContainerLow;
    public DynamicColor SurfaceContainer => _colorSpec.SurfaceContainer;
    public DynamicColor SurfaceContainerHigh => _colorSpec.SurfaceContainerHigh;
    public DynamicColor SurfaceContainerHighest => _colorSpec.SurfaceContainerHighest;
    public DynamicColor OnSurface => _colorSpec.OnSurface;
    public DynamicColor SurfaceVariant => _colorSpec.SurfaceVariant;
    public DynamicColor OnSurfaceVariant => _colorSpec.OnSurfaceVariant;
    public DynamicColor InverseSurface => _colorSpec.InverseSurface;
    public DynamicColor InverseOnSurface => _colorSpec.InverseOnSurface;
    public DynamicColor Outline => _colorSpec.Outline;
    public DynamicColor OutlineVariant => _colorSpec.OutlineVariant;
    public DynamicColor Shadow => _colorSpec.Shadow;
    public DynamicColor Scrim => _colorSpec.Scrim;
    public DynamicColor SurfaceTint => _colorSpec.SurfaceTint;
    public DynamicColor Primary => _colorSpec.Primary;
    public DynamicColor? PrimaryDim => _colorSpec.PrimaryDim;
    public DynamicColor OnPrimary => _colorSpec.OnPrimary;
    public DynamicColor PrimaryContainer => _colorSpec.PrimaryContainer;
    public DynamicColor OnPrimaryContainer => _colorSpec.OnPrimaryContainer;
    public DynamicColor InversePrimary => _colorSpec.InversePrimary;
    public DynamicColor Secondary => _colorSpec.Secondary;
    public DynamicColor? SecondaryDim => _colorSpec.SecondaryDim;
    public DynamicColor OnSecondary => _colorSpec.OnSecondary;
    public DynamicColor SecondaryContainer => _colorSpec.SecondaryContainer;
    public DynamicColor OnSecondaryContainer => _colorSpec.OnSecondaryContainer;
    public DynamicColor Tertiary => _colorSpec.Tertiary;
    public DynamicColor? TertiaryDim => _colorSpec.TertiaryDim;
    public DynamicColor OnTertiary => _colorSpec.OnTertiary;
    public DynamicColor TertiaryContainer => _colorSpec.TertiaryContainer;
    public DynamicColor OnTertiaryContainer => _colorSpec.OnTertiaryContainer;
    public DynamicColor Error => _colorSpec.Error;
    public DynamicColor? ErrorDim => _colorSpec.ErrorDim;
    public DynamicColor OnError => _colorSpec.OnError;
    public DynamicColor ErrorContainer => _colorSpec.ErrorContainer;
    public DynamicColor OnErrorContainer => _colorSpec.OnErrorContainer;
    public DynamicColor PrimaryFixed => _colorSpec.PrimaryFixed;
    public DynamicColor PrimaryFixedDim => _colorSpec.PrimaryFixedDim;
    public DynamicColor OnPrimaryFixed => _colorSpec.OnPrimaryFixed;
    public DynamicColor OnPrimaryFixedVariant => _colorSpec.OnPrimaryFixedVariant;
    public DynamicColor SecondaryFixed => _colorSpec.SecondaryFixed;
    public DynamicColor SecondaryFixedDim => _colorSpec.SecondaryFixedDim;
    public DynamicColor OnSecondaryFixed => _colorSpec.OnSecondaryFixed;
    public DynamicColor OnSecondaryFixedVariant => _colorSpec.OnSecondaryFixedVariant;
    public DynamicColor TertiaryFixed => _colorSpec.TertiaryFixed;
    public DynamicColor TertiaryFixedDim => _colorSpec.TertiaryFixedDim;
    public DynamicColor OnTertiaryFixed => _colorSpec.OnTertiaryFixed;
    public DynamicColor OnTertiaryFixedVariant => _colorSpec.OnTertiaryFixedVariant;
}
