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

using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Contrast;

/// <summary>
/// Utilities for calculating contrast ratios and finding suitable tones for accessibility.
/// </summary>
public class Contrast
{
    /// <summary>
    /// Calculates the contrast ratio between two tones.
    /// </summary>
    /// <param name="toneA">First tone value (0-100).</param>
    /// <param name="toneB">Second tone value (0-100).</param>
    /// <returns>Contrast ratio (1.0 to 21.0).</returns>
    public static double RatioOfTones(double toneA, double toneB)
    {
        toneA = Math.Clamp(toneA, 0.0, 100.0);
        toneB = Math.Clamp(toneB, 0.0, 100.0);
        return RatioOfYs(ColorUtils.YFromLstar(toneA), ColorUtils.YFromLstar(toneB));
    }

    private static double RatioOfYs(double y1, double y2)
    {
        var lighter = y1 > y2 ? y1 : y2;
        var darker = lighter == y2 ? y1 : y2;
        return (lighter + 5.0) / (darker + 5.0);
    }

    /// <summary>
    /// Finds a lighter tone that achieves the desired contrast ratio with the given tone.
    /// </summary>
    /// <param name="tone">The tone to contrast with (0-100).</param>
    /// <param name="ratio">The desired contrast ratio (1.0 to 21.0).</param>
    /// <returns>Lighter tone (0-100), or -1 if the ratio cannot be achieved.</returns>
    public static double Lighter(double tone, double ratio)
    {
        if (tone is < 0.0 or > 100.0)
            return -1.0;

        var darkY = ColorUtils.YFromLstar(tone);
        var lightY = ratio * (darkY + 5.0) - 5.0;
        var realContrast = RatioOfYs(lightY, darkY);
        var delta = Math.Abs(realContrast - ratio);
        if (realContrast < ratio && delta > 0.04)
            return -1;

        // Ensure gamut mapping, which requires a 'range' on tone, will still result
        // the correct ratio by darkening slightly.
        var returnValue = ColorUtils.LstarFromY(lightY) + 0.4;
        if (returnValue is < 0 or > 100)
            return -1;
        return returnValue;
    }

    /// <summary>
    /// Finds a darker tone that achieves the desired contrast ratio with the given tone.
    /// </summary>
    /// <param name="tone">The tone to contrast with (0-100).</param>
    /// <param name="ratio">The desired contrast ratio (1.0 to 21.0).</param>
    /// <returns>Darker tone (0-100), or -1 if the ratio cannot be achieved.</returns>
    public static double Darker(double tone, double ratio)
    {
        if (tone is < 0.0 or > 100.0)
            return -1.0;

        var lightY = ColorUtils.YFromLstar(tone);
        var darkY = (lightY + 5.0) / ratio - 5.0;
        var realContrast = RatioOfYs(lightY, darkY);

        var delta = Math.Abs(realContrast - ratio);
        if (realContrast < ratio && delta > 0.04)
            return -1;

        // Ensure gamut mapping, which requires a 'range' on tone, will still result
        // the correct ratio by darkening slightly.
        var returnValue = ColorUtils.LstarFromY(darkY) - 0.4;
        if (returnValue is < 0 or > 100)
            return -1;
        return returnValue;
    }

    /// <summary>
    /// Finds a lighter tone that achieves the desired contrast ratio, or returns 100 if impossible.
    /// </summary>
    /// <param name="tone">The tone to contrast with (0-100).</param>
    /// <param name="ratio">The desired contrast ratio (1.0 to 21.0).</param>
    /// <returns>Lighter tone (0-100), or 100 if the ratio cannot be achieved.</returns>
    public static double LighterUnsafe(double tone, double ratio)
    {
        var lighterSafe = Lighter(tone, ratio);
        return lighterSafe < 0.0 ? 100.0 : lighterSafe;
    }

    /// <summary>
    /// Finds a darker tone that achieves the desired contrast ratio, or returns 0 if impossible.
    /// </summary>
    /// <param name="tone">The tone to contrast with (0-100).</param>
    /// <param name="ratio">The desired contrast ratio (1.0 to 21.0).</param>
    /// <returns>Darker tone (0-100), or 0 if the ratio cannot be achieved.</returns>
    public static double DarkerUnsafe(double tone, double ratio)
    {
        var darkerSafe = Darker(tone, ratio);
        return darkerSafe < 0.0 ? 0.0 : darkerSafe;
    }
}