// Copyright 2021 Google LLC
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
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Blend;

/// <summary>
/// Functions for blending and harmonizing colors.
/// </summary>
public static class Blend
{
    /// <summary>
    /// Adjusts a design color to be more harmonious with a key source color.
    /// The design color's hue shifts toward the source color's hue by up to 15 degrees.
    /// </summary>
    /// <param name="designColor">The ARGB color to harmonize.</param>
    /// <param name="sourceColor">The ARGB source color to harmonize toward.</param>
    /// <returns>The harmonized ARGB color.</returns>
    public static ArgbColor Harmonize(ArgbColor designColor, ArgbColor sourceColor)
    {
        var fromHct = Hct.From(designColor);
        var toHct = Hct.From(sourceColor);
        var differenceDegrees = MathUtils.DifferenceDegrees(fromHct.Hue, toHct.Hue);
        var rotationDegrees = Math.Min(differenceDegrees * 0.5, 15.0);
        var outputHue = MathUtils.SanitizeDegrees(
            fromHct.Hue + rotationDegrees * MathUtils.RotationDirection(fromHct.Hue, toHct.Hue)
        );
        return Hct.From(outputHue, fromHct.Chroma, fromHct.Tone).Argb;
    }

    /// <summary>
    /// Blends the hue of two colors using CAM16 UCS color space.
    /// </summary>
    /// <param name="from">The ARGB color to blend from.</param>
    /// <param name="to">The ARGB color to blend to.</param>
    /// <param name="amount">The blend amount (0.0 to 1.0).</param>
    /// <returns>The blended ARGB color.</returns>
    public static ArgbColor HctHue(ArgbColor from, ArgbColor to, double amount)
    {
        var ucs = Cam16Ucs(from, to, amount);
        var ucsCam = Cam16.FromArgb(ucs);
        var fromCam = Cam16.FromArgb(from);
        var blended = Hct.From(ucsCam.Hue, fromCam.Chroma, ColorUtils.LstarFromArgb(from));
        return blended.Argb;
    }

    /// <summary>
    /// Blends two colors in CAM16 UCS color space.
    /// </summary>
    /// <param name="from">The ARGB color to blend from.</param>
    /// <param name="to">The ARGB color to blend to.</param>
    /// <param name="amount">The blend amount (0.0 to 1.0).</param>
    /// <returns>The blended ARGB color.</returns>
    public static ArgbColor Cam16Ucs(ArgbColor from, ArgbColor to, double amount)
    {
        var fromCam = Cam16.FromArgb(from);
        var toCam = Cam16.FromArgb(to);
        var fromJ = fromCam.Jstar;
        var fromA = fromCam.Astar;
        var fromB = fromCam.Bstar;
        var toJ = toCam.Jstar;
        var toA = toCam.Astar;
        var toB = toCam.Bstar;
        var jstar = fromJ + (toJ - fromJ) * amount;
        var astar = fromA + (toA - fromA) * amount;
        var bstar = fromB + (toB - fromB) * amount;
        return Cam16.FromUcs(jstar, astar, bstar).ToArgb();
    }
}
