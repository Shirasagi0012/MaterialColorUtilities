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

namespace MaterialColorUtilities.Utils;

/// <summary>
/// Color utility functions for converting between color spaces including
/// ARGB, XYZ, LAB, and L* (lightness).
/// </summary>
public static class ColorUtils
{
    private static readonly Matrix3x3 SrgbToXyz = new(
        0.41233895,
        0.35762064,
        0.18051042,
        0.2126,
        0.7152,
        0.0722,
        0.01932141,
        0.11916382,
        0.95034478
    );

    private static readonly Matrix3x3 XyzToSrgb = new(
        3.2413774792388685,
        -1.5376652402851851,
        -0.49885366846268053,
        -0.9691452513005321,
        1.8758853451067872,
        0.04156585616912061,
        0.05562093689691305,
        -0.20395524564742123,
        1.0571799111220335
    );

    /// <summary>
    /// The D65 standard illuminant white point in XYZ color space.
    /// </summary>
    internal static readonly Vector3D WhitePointD65 = new(95.047, 100.0, 108.883);

    /// <summary>
    /// Converts RGB components to ARGB color.
    /// </summary>
    /// <param name="red">Red component (0-255).</param>
    /// <param name="green">Green component (0-255).</param>
    /// <param name="blue">Blue component (0-255).</param>
    /// <returns>ARGB color with full opacity.</returns>
    internal static ArgbColor ArgbFromRgb(int red, int green, int blue)
    {
        return new ArgbColor(255, (byte)red, (byte)green, (byte)blue);
    }

    /// <summary>
    /// Converts linear RGB to ARGB color.
    /// </summary>
    /// <param name="linrgb">Linear RGB values as a Vector3D.</param>
    /// <returns>ARGB color.</returns>
    internal static ArgbColor ArgbFromLinrgb(Vector3D linrgb)
    {
        return Delinearized(linrgb);
    }

    /// <summary>
    /// Converts XYZ color space values to ARGB color.
    /// </summary>
    /// <param name="xyz">XYZ values as a Vector3D.</param>
    /// <returns>ARGB color.</returns>
    internal static ArgbColor ArgbFromXyz(Vector3D xyz)
    {
        // Convert from XYZ to linear sRGB, then delinearize to bytes.
        ref readonly var matrix = ref XyzToSrgb;
        var linearRGB = matrix * xyz;
        return Delinearized(linearRGB);
    }

    /// <summary>
    /// Converts ARGB color to XYZ color space values.
    /// </summary>
    /// <param name="argb">ARGB color.</param>
    /// <returns>XYZ values as a Vector3D.</returns>
    internal static Vector3D XyzFromArgb(ArgbColor argb)
    {
        // Convert from ARGB bytes to linear sRGB, then to XYZ.
        var linearRGB = Linearized(argb);
        ref readonly var matrix = ref SrgbToXyz;
        return matrix * linearRGB;
    }

    /// <summary>
    /// Converts LAB color space values to ARGB color.
    /// </summary>
    /// <param name="lab">LAB values as a Vector3D (L, a, b).</param>
    /// <returns>ARGB color.</returns>
    internal static ArgbColor ArgbFromLab(Vector3D lab)
    {
        ref readonly var whitePoint = ref WhitePointD65;

        var fy = (lab.X + 16.0) / 116.0;
        var fx = lab.Y / 500.0 + fy;
        var fz = fy - lab.Z / 200.0;

        var xyzNormalized = LabInvf(new Vector3D(fx, fy, fz));
        return ArgbFromXyz(xyzNormalized * whitePoint);
    }

    /// <summary>
    /// Converts ARGB color to LAB color space values.
    /// </summary>
    /// <param name="argb">ARGB color.</param>
    /// <returns>LAB values as a Vector3D (L, a, b).</returns>
    internal static Vector3D LabFromArgb(ArgbColor argb)
    {
        ref readonly var whitePoint = ref WhitePointD65;
        ref readonly var matrix = ref SrgbToXyz;

        var linearRGB = Linearized(argb);
        var xyz = matrix * linearRGB;
        var xyzNormalized = xyz / whitePoint;
        var f = LabF(xyzNormalized);
        return new Vector3D(116 * f.Y - 16, 500 * (f.X - f.Y), 200 * (f.Y - f.Z));
    }

    /// <summary>
    /// Converts L* (lightness) value to ARGB color (grayscale).
    /// </summary>
    /// <param name="lstar">L* value (0-100).</param>
    /// <returns>Grayscale ARGB color.</returns>
    internal static ArgbColor ArgbFromLstar(double lstar)
    {
        var y = YFromLstar(lstar);
        var component = Delinearized(y);
        return new ArgbColor(255, component, component, component);
    }

    /// <summary>
    /// Calculates L* (lightness) from ARGB color.
    /// </summary>
    /// <param name="argb">ARGB color.</param>
    /// <returns>L* value (0-100).</returns>
    internal static double LstarFromArgb(ArgbColor argb)
    {
        var y = XyzFromArgb(argb).Y;
        return 116.0 * LabF(y / 100.0) - 16.0;
    }

    /// <summary>
    /// Converts L* (lightness) to Y in XYZ color space.
    /// </summary>
    /// <param name="lstar">L* value (0-100).</param>
    /// <returns>Y value in XYZ.</returns>
    internal static double YFromLstar(double lstar)
    {
        return 100.0 * LabInvf((lstar + 16.0) / 116.0);
    }

    /// <summary>
    /// Converts Y in XYZ color space to L* (lightness).
    /// </summary>
    /// <param name="y">Y value in XYZ.</param>
    /// <returns>L* value (0-100).</returns>
    internal static double LstarFromY(double y)
    {
        return LabF(y / 100.0) * 116.0 - 16.0;
    }

    internal static double Linearized(byte rgbComponent)
    {
        var normalized = rgbComponent / 255.0;
        return normalized <= 0.040449936
            ? normalized / 12.92 * 100.0
            : Math.Pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
    }

    internal static Vector3D Linearized(ArgbColor argb)
    {
        return new Vector3D(Linearized(argb.Red), Linearized(argb.Green), Linearized(argb.Blue));
    }

    internal static byte Delinearized(double linearComponent)
    {
        var normalized = linearComponent / 100.0;
        var delinearized =
            normalized <= 0.0031308
                ? normalized * 12.92
                : 1.055 * Math.Pow(normalized, 1.0 / 2.4) - 0.055;
        return (byte)
            Math.Round(Math.Clamp(delinearized * 255.0, 0.0, 255.0), MidpointRounding.AwayFromZero);
    }

    internal static ArgbColor Delinearized(Vector3D linearRgb)
    {
        return new ArgbColor(
            255,
            Delinearized(linearRgb.X),
            Delinearized(linearRgb.Y),
            Delinearized(linearRgb.Z)
        );
    }

    private static Vector3D LabF(Vector3D t)
    {
        return new Vector3D(LabF(t.X), LabF(t.Y), LabF(t.Z));
    }

    private static double LabF(double t)
    {
        const double e = 216.0 / 24389.0;
        const double kappa = 24389.0 / 27.0;
        return t > e ? Math.Pow(t, 1.0 / 3.0) : (kappa * t + 16) / 116;
    }

    private static Vector3D LabInvf(Vector3D ft)
    {
        return new Vector3D(LabInvf(ft.X), LabInvf(ft.Y), LabInvf(ft.Z));
    }

    private static double LabInvf(double ft)
    {
        const double e = 216.0 / 24389.0;
        const double kappa = 24389.0 / 27.0;
        var ft3 = ft * ft * ft;
        return ft3 > e ? ft3 : (116 * ft - 16) / kappa;
    }
}