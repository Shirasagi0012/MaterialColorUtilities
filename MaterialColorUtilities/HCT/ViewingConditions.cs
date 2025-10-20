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
/// In traditional color spaces, a color can be identified solely by the
/// observer's measurement of the color. Color appearance models such as CAM16
/// also use information about the environment where the color was
/// observed, known as the viewing conditions.
///
/// For example, white under the traditional assumption of a midday sun white
/// point is accurately measured as a slightly chromatic blue by CAM16.
/// (roughly, hue 203, chroma 3, lightness 100)
///
/// This struct caches intermediate values of the CAM16 conversion process that
/// depend only on viewing conditions, enabling speed ups.
/// </summary>
public readonly record struct ViewingConditions(
    Vector3D WhitePoint,
    double AdaptingLuminance,
    double BackgroundLstar,
    double Surround,
    bool DiscountingIlluminant,
    double BackgroundYToWhitePointY,
    double Aw,
    double Nbb,
    double Ncb,
    double C,
    double NC,
    Vector3D DrgbInverse,
    Vector3D RgbD,
    double Fl,
    double FLRoot,
    double Z
)
{
    // XYZ to cone response transformation matrix (same as in Cam16)
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

    readonly private static ViewingConditions _standard = Make();

    // private static readonly ViewingConditions _srgb = Make();

    public static ref readonly ViewingConditions Standard => ref _standard;
    public static ref readonly ViewingConditions SRgb => ref _standard;

    /// <summary>
    /// Convenience factory method for ViewingConditions.
    ///
    /// Parameters affecting color appearance include:
    /// </summary>
    /// <param name="whitePoint">Coordinates of white in XYZ color space. If null, uses D65 white point.</param>
    /// <param name="adaptingLuminance">Light strength, in lux. If negative, defaults calculated value.</param>
    /// <param name="backgroundLstar">Average luminance of 10 degrees around color.</param>
    /// <param name="surround">Brightness of the entire environment.</param>
    /// <param name="discountingIlluminant">Whether eyes have adjusted to lighting.</param>
    /// <returns>ViewingConditions with the specified parameters</returns>
    public static ViewingConditions Make(
        Vector3D? whitePoint = null,
        double adaptingLuminance = -1.0,
        double backgroundLstar = 50.0,
        double surround = 2.0,
        bool discountingIlluminant = false
    )
    {
        var wp = whitePoint ?? ColorUtils.WhitePointD65;

        adaptingLuminance =
            adaptingLuminance > 0.0
                ? adaptingLuminance
                : 200.0 / Math.PI * ColorUtils.YFromLstar(50.0) / 100.0;

        // A background of pure black is non-physical and leads to infinities that
        // represent the idea that any color viewed in pure black can't be seen.
        backgroundLstar = Math.Max(0.1, backgroundLstar);

        // Transform test illuminant white in XYZ to 'cone'/'rgb' responses
        var whitePointConeResponse = XyzToConeMatrix * wp;

        // Scale input surround, domain (0, 2), to CAM16 surround, domain (0.8, 1.0)
        System.Diagnostics.Debug.Assert(surround >= 0.0 && surround <= 2.0);
        var f = 0.8 + surround / 10.0;

        // "Exponential non-linearity"
        var c =
            f >= 0.9
                ? MathUtils.Lerp(0.59, 0.69, (f - 0.9) * 10.0)
                : MathUtils.Lerp(0.525, 0.59, (f - 0.8) * 10.0);

        // Calculate degree of adaptation to illuminant
        var d = discountingIlluminant
            ? 1.0
            : f * (1.0 - 1.0 / 3.6 * Math.Exp((-adaptingLuminance - 42.0) / 92.0));

        // Per Li et al, if D is greater than 1 or less than 0, set it to 1 or 0.
        d = Math.Clamp(d, 0.0, 1.0);

        // chromatic induction factor
        var nc = f;

        // Cone responses to the whitePoint, r/g/b/W, adjusted for discounting
        //
        // Why use 100.0 instead of the white point's relative luminance?
        //
        // Some papers and implementations, for both CAM02 and CAM16, use the Y
        // value of the reference white instead of 100. Fairchild's Color Appearance
        // Models (3rd edition) notes that this is in error: it was included in the
        // CIE 2004a report on CIECAM02, but, later parts of the conversion process
        // account for scaling of appearance relative to the white point relative
        // luminance. This part should simply use 100 as luminance.
        var rgbD = d * (new Vector3D(100.0, 100.0, 100.0) / whitePointConeResponse) + (1.0 - d);

        // Factor used in calculating meaningful factors
        var k = 1.0 / (5.0 * adaptingLuminance + 1.0);
        var k4 = k * k * k * k;
        var k4F = 1.0 - k4;

        // Luminance-level adaptation factor
        var fl =
            k4 * adaptingLuminance + 0.1 * k4F * k4F * Math.Pow(5.0 * adaptingLuminance, 1.0 / 3.0);

        // Intermediate factor, ratio of background relative luminance to white relative luminance
        var n = ColorUtils.YFromLstar(backgroundLstar) / wp.Y;

        // Base exponential nonlinearity
        // note Schlomer 2018 has a typo and uses 1.58, the correct factor is 1.48
        var z = 1.48 + Math.Sqrt(n);

        // Luminance-level induction factors
        var nbb = 0.725 / Math.Pow(n, 0.2);
        var ncb = nbb;

        // Discounted cone responses to the white point, adjusted for post-adaptation
        // perceptual nonlinearities - vectorized
        var rgbAFactors = (fl * rgbD * whitePointConeResponse / 100.0).Pow(0.42);
        var rgbA = 400.0 * rgbAFactors / (rgbAFactors + 27.13);

        var aw = (40.0 * rgbA.X + 20.0 * rgbA.Y + rgbA.Z) / 20.0 * nbb;

        return new ViewingConditions(
            wp,
            adaptingLuminance,
            backgroundLstar,
            surround,
            discountingIlluminant,
            n,
            aw,
            nbb,
            ncb,
            c,
            nc,
            new Vector3D(0.0, 0.0, 0.0), // Placeholder - will be calculated when needed
            rgbD,
            fl,
            Math.Pow(fl, 0.25),
            z
        );
    }
}