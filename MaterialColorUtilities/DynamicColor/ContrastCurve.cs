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

using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.DynamicColor;

/// <summary>
/// A class containing a value that changes with the contrast level.
///
/// Usually represents the contrast requirements for a dynamic color on its
/// background. The four values correspond to values for contrast levels
/// -1.0, 0.0, 0.5, and 1.0, respectively.
/// </summary>
public class ContrastCurve
{
    public readonly double Low;
    public readonly double Normal;
    public readonly double Medium;
    public readonly double High;

    /// <summary>
    /// Creates a ContrastCurve object.
    /// </summary>
    /// <param name="low">Value for contrast level -1.0</param>
    /// <param name="normal">Value for contrast level 0.0</param>
    /// <param name="medium">Value for contrast level 0.5</param>
    /// <param name="high">Value for contrast level 1.0</param>
    public ContrastCurve(double low, double normal, double medium, double high)
    {
        Low = low;
        Normal = normal;
        Medium = medium;
        High = high;
    }

    /// <summary>
    /// Returns the value at a given contrast level.
    /// </summary>
    /// <param name="contrastLevel">The contrast level. 0.0 is the default (normal);
    /// -1.0 is the lowest; 1.0 is the highest.</param>
    /// <returns>The value. For contrast ratios, a number between 1.0 and 21.0.</returns>
    public double Get(double contrastLevel)
    {
        return contrastLevel switch
        {
            <= -1.0 => Low,
            < 0.0 => MathUtils.Lerp(Low, Normal, (contrastLevel - -1) / 1),
            < 0.5 => MathUtils.Lerp(Normal, Medium, (contrastLevel - 0) / 0.5),
            < 1.0 => MathUtils.Lerp(Medium, High, (contrastLevel - 0.5) / 0.5),
            _ => High
        };
    }
}