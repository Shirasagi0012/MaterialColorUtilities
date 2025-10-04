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
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Scheme;

/// <summary>
/// A Dynamic Color theme that is intentionally detached from the input color.
/// </summary>
public class SchemeExpressive : DynamicScheme
{
    /// <summary>
    /// Hues used at breakpoints such that designers can specify a hue rotation
    /// that occurs at a given break point.
    /// </summary>
    private static readonly double[] Hues = [0, 21, 51, 121, 151, 191, 271, 321, 360];

    /// <summary>
    /// Hue rotations of the Secondary TonalPalette, corresponding to the
    /// breakpoints in Hues.
    /// </summary>
    private static readonly double[] SecondaryRotations = [45, 95, 45, 20, 45, 90, 45, 45, 45];

    /// <summary>
    /// Hue rotations of the Tertiary TonalPalette, corresponding to the
    /// breakpoints in Hues.
    /// </summary>
    private static readonly double[] TertiaryRotations = [120, 120, 20, 45, 20, 15, 20, 120, 120];

    public SchemeExpressive(Hct sourceColorHct, bool isDark, double contrastLevel)
        : base(
            sourceColorHct,
            Variant.Expressive,
            isDark,
            contrastLevel,
            new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 240.0), 40.0),
            new TonalPalette(GetRotatedHue(sourceColorHct, Hues, SecondaryRotations), 24.0),
            new TonalPalette(GetRotatedHue(sourceColorHct, Hues, TertiaryRotations), 32.0),
            new TonalPalette(sourceColorHct.Hue + 15.0, 8.0),
            new TonalPalette(sourceColorHct.Hue + 15.0, 12.0)
        )
    {
    }
}