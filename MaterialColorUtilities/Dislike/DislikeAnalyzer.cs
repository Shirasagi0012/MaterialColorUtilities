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

using System;
using System.Collections.Generic;
using System.Text;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Dislike;

/// <summary>
/// Checks and fixes universally disliked colors.
/// Color science studies of color preference indicate universal distaste for
/// dark yellow-greens, and also show this is correlated to distate for biological
/// waste and rotting food.
///
/// See Palmer and Schloss, 2010 or Schloss and Palmer's Chapter 21 in Handbook of Color Psychology (2015).
/// </summary>
public static class DislikeAnalyzer
{
    /// <summary>
    /// Determines whether a color is considered disliked.
    /// </summary>
    /// <param name="hct">The HCT color to check.</param>
    /// <returns>True if the color is disliked (dark yellow-green).</returns>
    public static bool IsDisliked(Hct hct)
    {
        var huePasses = Math.Round(hct.Hue, MidpointRounding.AwayFromZero) >= 90.0 && Math.Round(hct.Hue, MidpointRounding.AwayFromZero) <= 111.0;
        var chromaPasses = Math.Round(hct.Chroma, MidpointRounding.AwayFromZero) > 16.0;
        var tonePasses = Math.Round(hct.Tone, MidpointRounding.AwayFromZero) < 65.0;

        return huePasses && chromaPasses && tonePasses;
    }

    /// <summary>
    /// Adjusts a disliked color to make it more appealing if needed.
    /// If the color is disliked, the tone is increased to 70.0 to make it lighter and more acceptable.
    /// </summary>
    /// <param name="hct">The HCT color to fix if disliked.</param>
    /// <returns>The original color if not disliked, or a fixed version if disliked.</returns>
    public static Hct FixIfDisliked(Hct hct)
    {
        return IsDisliked(hct) ? Hct.From(hct.Hue, hct.Chroma, 70.0) : hct;
    }
}