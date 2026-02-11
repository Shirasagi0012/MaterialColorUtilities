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

using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.HCT;

/// <summary>
/// CAM16, a color appearance model. Colors are not just defined by their hex
/// code, but rather, a hex code and viewing conditions.
///
/// CAM16 instances also have coordinates in the CAM16-UCS space, called J*, a*,
/// b*, or jstar, astar, bstar in code. CAM16-UCS is included in the CAM16
/// specification, and should be used when measuring distances between colors.
///
/// In traditional color spaces, a color can be identified solely by the
/// observer's measurement of the color. Color appearance models such as CAM16
/// also use information about the environment where the color was
/// observed, known as the viewing conditions.
///
/// For example, white under the traditional assumption of a midday sun white
/// point is accurately measured as a slightly chromatic blue by CAM16.
/// (roughly, hue 203, chroma 3, lightness 100)
/// </summary>
internal readonly record struct Cam16(
    // Like red, orange, yellow, green, etc.
    double Hue,
    // Informally, colorfulness / color intensity. Like saturation in HSL, except perceptually accurate.
    double Chroma,
    // Lightness
    double J,
    // >Brightness; ratio of lightness to white point's lightness
    double Q,
    // Colorfulness
    double M,
    // Saturation; ratio of chroma to white point's chroma
    double S,
    // CAM16-UCS J coordinate
    double Jstar,
    // CAM16-UCS a coordinate
    double Astar,
    // CAM16-UCS b coordinate
    double Bstar
)
{
    // Transformation matrices as static constants
    public readonly static Matrix3x3 XyzToConeMatrix = new(
        0.401288,
        0.650173,
        -0.051461,
        -0.250268,
        1.204414,
        0.045854,
        -0.002079,
        0.048952,
        0.953127
    );

    public readonly static Matrix3x3 ConeToXyzMatrix = new(
        1.86206786,
        -1.01125463,
        0.14918677,
        0.38752654,
        0.62144744,
        -0.00897398,
        -0.01584150,
        -0.03412294,
        1.04996444
    );

    /// <summary>
    /// CAM16 instances also have coordinates in the CAM16-UCS space, called J*,
    /// a*, b*, or jstar, astar, bstar in code. CAM16-UCS is included in the CAM16
    /// specification, and should be used when measuring distances between colors.
    /// </summary>
    public double Distance(Cam16 other)
    {
        var dJ = Jstar - other.Jstar;
        var dA = Astar - other.Astar;
        var dB = Bstar - other.Bstar;
        var dEPrime = Math.Sqrt(dJ * dJ + dA * dA + dB * dB);
        var dE = 1.41 * Math.Pow(dEPrime, 0.63);
        return dE;
    }

    /// <summary>
    /// Convert ARGB to CAM16, assuming the color was viewed in default viewing conditions.
    /// </summary>
    public static Cam16 FromArgb(ArgbColor argb)
    {
        return FromArgbInViewingConditions(argb, ViewingConditions.SRgb);
    }

    /// <summary>
    /// Given viewing conditions, convert ARGB to CAM16.
    /// </summary>
    public static Cam16 FromArgbInViewingConditions(
        ArgbColor argb,
        ViewingConditions viewingConditions
    )
    {
        // Transform ARGB to XYZ
        var xyz = ColorUtils.XyzFromArgb(argb);
        return FromXyzInViewingConditions(xyz, viewingConditions);
    }

    /// <summary>
    /// Given color expressed in XYZ and viewed in viewing conditions, convert to CAM16.
    /// </summary>
    public static Cam16 FromXyzInViewingConditions(
        Vector3D xyz,
        ViewingConditions viewingConditions
    )
    {
        // Transform XYZ to 'cone'/'rgb' responses
        var coneResponse = XyzToConeMatrix * xyz;

        // Discount illuminant
        var discountedResponse = coneResponse * viewingConditions.RgbD;

        // Chromatic adaptation - vectorized operations
        var adaptationFactors = (discountedResponse.Abs() * viewingConditions.Fl / 100.0).Pow(0.42);
        var adaptedResponse =
            discountedResponse.Signum() * 400.0 * adaptationFactors / (adaptationFactors + 27.13);

        // Redness-greenness and yellowness-blueness
        var a = (11.0 * adaptedResponse.X + -12.0 * adaptedResponse.Y + adaptedResponse.Z) / 11.0;
        var b = (adaptedResponse.X + adaptedResponse.Y - 2.0 * adaptedResponse.Z) / 9.0;

        // Auxiliary components
        var u =
            (20.0 * adaptedResponse.X + 20.0 * adaptedResponse.Y + 21.0 * adaptedResponse.Z) / 20.0;
        var p2 = (40.0 * adaptedResponse.X + 20.0 * adaptedResponse.Y + adaptedResponse.Z) / 20.0;

        // Hue
        var atan2 = Math.Atan2(b, a);
        var atanDegrees = atan2 * 180.0 / Math.PI;
        var hue = MathUtils.SanitizeDegrees(atanDegrees);
        var hueRadians = hue * Math.PI / 180.0;
        System.Diagnostics.Debug.Assert(hue >= 0 && hue < 360, $"hue was really {hue}");

        // Achromatic response to color
        var ac = p2 * viewingConditions.Nbb;

        // CAM16 lightness and brightness
        var J =
            100.0 * Math.Pow(ac / viewingConditions.Aw, viewingConditions.C * viewingConditions.Z);
        var Q =
            4.0
            / viewingConditions.C
            * Math.Sqrt(J / 100.0)
            * (viewingConditions.Aw + 4.0)
            * viewingConditions.FLRoot;

        var huePrime = hue < 20.14 ? hue + 360 : hue;
        var eHue = 1.0 / 4.0 * (Math.Cos(huePrime * Math.PI / 180.0 + 2.0) + 3.8);
        var p1 = 50000.0 / 13.0 * eHue * viewingConditions.NC * viewingConditions.Ncb;
        var t = p1 * Math.Sqrt(a * a + b * b) / (u + 0.305);
        var alpha =
            Math.Pow(t, 0.9)
            * Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.BackgroundYToWhitePointY), 0.73);

        // CAM16 chroma, colorfulness, saturation
        var C = alpha * Math.Sqrt(J / 100.0);
        var M = C * viewingConditions.FLRoot;
        var s = 50.0 * Math.Sqrt(alpha * viewingConditions.C / (viewingConditions.Aw + 4.0));

        // CAM16-UCS components
        var jstar = (1.0 + 100.0 * 0.007) * J / (1.0 + 0.007 * J);
        var mstar = Math.Log(1.0 + 0.0228 * M) / 0.0228;
        var astar = mstar * Math.Cos(hueRadians);
        var bstar = mstar * Math.Sin(hueRadians);

        return new Cam16(hue, C, J, Q, M, s, jstar, astar, bstar);
    }

    /// <summary>
    /// Create a CAM16 color from lightness J, chroma C, and hue H,
    /// assuming the color was viewed in default viewing conditions.
    /// </summary>
    public static Cam16 FromJch(double j, double c, double h)
    {
        return FromJchInViewingConditions(j, c, h, ViewingConditions.SRgb);
    }

    /// <summary>
    /// Create a CAM16 color from lightness J, chroma C, and hue H,
    /// in viewing conditions.
    /// </summary>
    public static Cam16 FromJchInViewingConditions(
        double J,
        double C,
        double h,
        ViewingConditions viewingConditions
    )
    {
        var Q =
            4.0
            / viewingConditions.C
            * Math.Sqrt(J / 100.0)
            * (viewingConditions.Aw + 4.0)
            * viewingConditions.FLRoot;
        var M = C * viewingConditions.FLRoot;
        var alpha = C / Math.Sqrt(J / 100.0);
        var s = 50.0 * Math.Sqrt(alpha * viewingConditions.C / (viewingConditions.Aw + 4.0));

        var hueRadians = h * Math.PI / 180.0;
        var jstar = (1.0 + 100.0 * 0.007) * J / (1.0 + 0.007 * J);
        var mstar = 1.0 / 0.0228 * Math.Log(1.0 + 0.0228 * M);
        var astar = mstar * Math.Cos(hueRadians);
        var bstar = mstar * Math.Sin(hueRadians);

        return new Cam16(h, C, J, Q, M, s, jstar, astar, bstar);
    }

    /// <summary>
    /// Create a CAM16 color from CAM16-UCS coordinates jstar, astar, bstar,
    /// assuming the color was viewed in default viewing conditions.
    /// </summary>
    public static Cam16 FromUcs(double jstar, double astar, double bstar)
    {
        return FromUcsInViewingConditions(jstar, astar, bstar, ViewingConditions.Standard);
    }

    /// <summary>
    /// Create a CAM16 color from CAM16-UCS coordinates jstar, astar, bstar,
    /// in viewing conditions.
    /// </summary>
    public static Cam16 FromUcsInViewingConditions(
        double jstar,
        double astar,
        double bstar,
        ViewingConditions viewingConditions
    )
    {
        var a = astar;
        var b = bstar;
        var m = Math.Sqrt(a * a + b * b);
        var M = (Math.Exp(m * 0.0228) - 1.0) / 0.0228;
        var c = M / viewingConditions.FLRoot;
        var h = Math.Atan2(b, a) * (180.0 / Math.PI);
        if (h < 0.0)
            h += 360.0;
        var j = jstar / (1 - (jstar - 100) * 0.007);

        return FromJchInViewingConditions(j, c, h, viewingConditions);
    }

    /// <summary>
    /// ARGB representation of color, assuming the color was viewed in default viewing conditions.
    /// </summary>
    public ArgbColor ToArgb()
    {
        return ViewedInConditions(ViewingConditions.SRgb);
    }

    /// <summary>
    /// ARGB representation of a color, given the color was viewed in viewing conditions.
    /// </summary>
    public ArgbColor ViewedInConditions(ViewingConditions viewingConditions)
    {
        var xyz = XyzInViewingConditions(viewingConditions);
        return ColorUtils.ArgbFromXyz(xyz);
    }

    /// <summary>
    /// XYZ representation of CAM16 seen in viewing conditions.
    /// </summary>
    public Vector3D XyzInViewingConditions(ViewingConditions viewingConditions)
    {
        var alpha = Chroma == 0.0 || J == 0.0 ? 0.0 : Chroma / Math.Sqrt(J / 100.0);

        var t = Math.Pow(
            alpha
            / Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.BackgroundYToWhitePointY), 0.73),
            1.0 / 0.9
        );
        var hRad = Hue * Math.PI / 180.0;

        var eHue = 0.25 * (Math.Cos(hRad + 2.0) + 3.8);
        var ac =
            viewingConditions.Aw
            * Math.Pow(J / 100.0, 1.0 / viewingConditions.C / viewingConditions.Z);
        var p1 = eHue * (50000.0 / 13.0) * viewingConditions.NC * viewingConditions.Ncb;

        var p2 = ac / viewingConditions.Nbb;

        var hSin = Math.Sin(hRad);
        var hCos = Math.Cos(hRad);

        var gamma = 23.0 * (p2 + 0.305) * t / (23.0 * p1 + 11 * t * hCos + 108.0 * t * hSin);
        var a = gamma * hCos;
        var b = gamma * hSin;

        var p2Vector = new Vector3D(460.0 * p2, 460.0 * p2, 460.0 * p2);
        var coefficientsA = new Vector3D(451.0 * a, -891.0 * a, -220.0 * a);
        var coefficientsB = new Vector3D(288.0 * b, -261.0 * b, -6300.0 * b);
        var rgbA = (p2Vector + coefficientsA + coefficientsB) / 1403.0;

        var absRgbA = rgbA.Abs();
        var rgbCBase = (absRgbA * 27.13 / (400.0 - absRgbA)).Max(0.0);
        var rgbC = rgbA.Signum() * (100.0 / viewingConditions.Fl) * rgbCBase.Pow(1.0 / 0.42);
        var rgbF = rgbC / viewingConditions.RgbD;

        var xyz = ConeToXyzMatrix * rgbF;

        return xyz;
    }
}
