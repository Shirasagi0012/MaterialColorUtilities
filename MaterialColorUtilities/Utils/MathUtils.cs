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

using System.Numerics;
using System.Runtime.CompilerServices;

namespace MaterialColorUtilities.Utils;

/// <summary>
/// Mathematical utility functions for color calculations.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Returns the sign of a number: -1 for negative, 0 for zero, 1 for positive.
    /// </summary>
    /// <param name="value">The value to get the sign of</param>
    /// <returns>-1, 0, or 1</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Signum(double value)
    {
        return value < 0.0 ? -1.0
            : value > 0.0 ? 1.0
            : 0.0;
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="start">Start value</param>
    /// <param name="end">End value</param>
    /// <param name="amount">Interpolation factor (0.0 to 1.0)</param>
    /// <returns>Interpolated value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Lerp(double start, double end, double amount)
    {
        return start + (end - start) * amount;
    }

    /// <summary>
    /// Sanitizes degrees to be within the range [0, 360).
    /// </summary>
    /// <typeparam name="T">Numeric type that supports INumber interface.</typeparam>
    /// <param name="degrees">Angle in degrees.</param>
    /// <returns>Normalized angle in [0, 360) range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T SanitizeDegrees<T>(T degrees)
        where T : INumber<T>
    {
        var fullCircle = T.CreateChecked(360.0);
        var result = degrees % fullCircle;
        if (result < T.Zero)
            result += fullCircle;
        return result;
    }

    /// <summary>
    /// Returns the direction of rotation from one angle to another.
    /// </summary>
    /// <param name="from">Starting angle in degrees.</param>
    /// <param name="to">Ending angle in degrees.</param>
    /// <returns>1.0 for clockwise rotation, -1.0 for counter-clockwise.</returns>
    internal static double RotationDirection(double from, double to)
    {
        var increasingDifference = SanitizeDegrees(to - from);
        return increasingDifference <= 180.0 ? 1.0 : -1.0;
    }

    /// <summary>
    /// Returns the difference between two angles in degrees.
    /// The result is always in the range [0, 180].
    /// </summary>
    /// <param name="a">First angle in degrees.</param>
    /// <param name="b">Second angle in degrees.</param>
    /// <returns>Difference in degrees [0, 180].</returns>
    internal static double DifferenceDegrees(double a, double b)
    {
        return 180.0 - Math.Abs(Math.Abs(a - b) - 180.0);
    }
}
