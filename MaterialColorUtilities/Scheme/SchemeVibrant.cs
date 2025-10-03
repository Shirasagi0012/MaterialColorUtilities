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

using MaterialColorUtilities.DynamicColor;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;

namespace MaterialColorUtilities.Scheme;

/// <summary>
/// A Dynamic Color theme that maxes out colorfulness at each position in the
/// Primary TonalPalette.
/// </summary>
public class SchemeVibrant(Hct sourceColorHct, bool isDark, double contrastLevel)
    : DynamicScheme(
        sourceColorHct,
        Variant.Vibrant,
        isDark,
        contrastLevel,
        new TonalPalette(sourceColorHct.Hue, 200.0),
        new TonalPalette(GetRotatedHue(sourceColorHct, Hues, SecondaryRotations), 24.0),
        new TonalPalette(GetRotatedHue(sourceColorHct, Hues, TertiaryRotations), 32.0),
        new TonalPalette(sourceColorHct.Hue, 10.0),
        new TonalPalette(sourceColorHct.Hue, 12.0)
    )
{
    /// <summary>
    /// Hues used at breakpoints such that designers can specify a hue rotation
    /// that occurs at a given break point.
    /// </summary>
    private static readonly double[] Hues = [0, 41, 61, 101, 131, 181, 251, 301, 360];

    /// <summary>
    /// Hue rotations of the Secondary TonalPalette, corresponding to the
    /// breakpoints in Hues.
    /// </summary>
    private static readonly double[] SecondaryRotations = [18, 15, 10, 12, 15, 18, 15, 12, 12];

    /// <summary>
    /// Hue rotations of the Tertiary TonalPalette, corresponding to the
    /// breakpoints in Hues.
    /// </summary>
    private static readonly double[] TertiaryRotations = [35, 30, 20, 25, 30, 35, 30, 25, 25];
}
