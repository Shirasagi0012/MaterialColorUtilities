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

using System.Diagnostics.CodeAnalysis;
using MaterialColorUtilities.Utils;
using Hct = MaterialColorUtilities.HCT.Hct;

namespace MaterialColorUtilities.Temperature;

/// <summary>
/// Manages caching and calculation of color temperature-related operations.
///
/// Design utilities using color temperature theory.
///
/// Analogous colors, complementary color, and cache to efficiently, lazily,
/// generate data for calculations.
///
/// A traditional color wheel has red at 0 degrees and blue at 240 degrees.
/// This color wheel uses red at 0 degrees and blue at 270 degrees, like HSL,
/// but uses CAM16 hue instead of HSL hue.
///
/// The majority of color science and HCT users prefer HSL's definition
/// of hue.
///
/// Generates example analogous colors from a source color.
///
/// Color temperature is based on LAB b* a* coordinates.
/// </summary>
public class TemperatureCache(Hct input)
{
    /// <summary>
    /// The input HCT color for which temperature operations are performed.
    /// </summary>
    public Hct Input { get; set; } = input;

    /// <summary>
    /// Gets HCT colors sorted by their temperature from coldest to warmest.
    /// </summary>
    public List<Hct> HctsByTemp
    {
        get
        {
            if (_hctsByTemp.Any())
                return _hctsByTemp;

            List<Hct> hcts = [.. HctsByHue, Input];
            var temperaturesByHct = TempsByHct;
            hcts.Sort((a, b) => temperaturesByHct[a].CompareTo(temperaturesByHct[b]));
            _hctsByTemp = hcts;
            return _hctsByTemp;
        }
    }

    public List<Hct> HctsByHue
    {
        get
        {
            if (_hctsByHue.Any())
                return _hctsByHue;

            List<Hct> hcts = [];
            for (var hue = 0.0; hue <= 360.0; hue += 1.0)
            {
                var colorAtHue = Hct.From(hue, Input.Chroma, Input.Tone);
                hcts.Add(colorAtHue);
            }

            _hctsByHue = [.. hcts];
            return _hctsByHue;
        }
    }

    public Dictionary<Hct, double> TempsByHct
    {
        get
        {
            if (_tempsByHct.Any())
                return _tempsByHct;
            List<Hct> allHcts = [.. HctsByHue, Input];

            var temperaturesByHct = new Dictionary<Hct, double>();
            foreach (var e in allHcts)
            {
                temperaturesByHct[e] = RawTemperature(e);
            }

            _tempsByHct = temperaturesByHct;
            return _tempsByHct;
        }
    }

    /// <summary>
    /// Gets the complement of the input color.
    /// </summary>
    [NotNull]
    public Hct? Complement
    {
        get
        {
            if (field is { } c)
                return c;

            var coldestHue = Coldest.Hue;
            var coldestTemp = TempsByHct[Coldest];

            var warmestHue = Warmest.Hue;
            var warmestTemp = TempsByHct[Warmest];
            var range = warmestTemp - coldestTemp;
            var startHueIsColdestToWarmest = IsBetween(Input.Hue, coldestHue, warmestHue);
            var startHue = startHueIsColdestToWarmest ? warmestHue : coldestHue;
            var endHue = startHueIsColdestToWarmest ? coldestHue : warmestHue;
            const double directionOfRotation = 1.0;
            var smallestError = 1000.0;
            var answer = HctsByHue[(int)Math.Round(Input.Hue, MidpointRounding.AwayFromZero)];

            var complementRelativeTemp = 1.0 - InputRelativeTemperature;
            // Find the color in the other section, closest to the inverse percentile
            // of the input color. This is the complement.
            for (var hueAddend = 0.0; hueAddend <= 360.0; hueAddend += 1.0)
            {
                var hue = MathUtils.SanitizeDegrees(startHue + directionOfRotation * hueAddend);
                if (!IsBetween(hue, startHue, endHue))
                    continue;
                var possibleAnswer = HctsByHue[(int)Math.Round(hue, MidpointRounding.AwayFromZero)];
                var relativeTemp = (_tempsByHct[possibleAnswer] - coldestTemp) / range;
                var error = Math.Abs(complementRelativeTemp - relativeTemp);
                if (error < smallestError)
                {
                    smallestError = error;
                    answer = possibleAnswer;
                }
            }

            field = answer;
            return (Hct)field!;
        }
    }

    private double _inputRelativeTemperature = -1.0;
    private Dictionary<Hct, double> _tempsByHct = [];
    private List<Hct> _hctsByHue = [];
    private List<Hct> _hctsByTemp = [];

    public Hct Warmest => HctsByTemp.Last();
    public Hct Coldest => HctsByTemp.First();

    /// <summary>
    /// Generates a set of analogous colors.
    ///
    /// An analogous color set is a set of colors that all have the same hue
    /// distance from each other, determined by rotating around the color wheel.
    /// </summary>
    /// <param name="count">The number of colors to generate (default is 5).</param>
    /// <param name="divisions">The number of divisions on the color wheel (default is 12).</param>
    /// <returns>A list of analogous HCT colors.</returns>
    public List<Hct> Analogous(int count = 5, int divisions = 12)
    {
        var startHue = (int)Math.Round(Input.Hue, MidpointRounding.AwayFromZero);
        var startHct = HctsByHue[startHue];
        var lastTemp = RelativeTemperature(startHct);
        List<Hct> allColors = [startHct];

        var absoluteTotalTempDelta = 0.0;
        for (var i = 0; i < 360; i++)
        {
            var hue = MathUtils.SanitizeDegrees(startHue + i);
            var hct = HctsByHue[hue];
            var temp = RelativeTemperature(hct);
            var tempDelta = Math.Abs(temp - lastTemp);
            lastTemp = temp;
            absoluteTotalTempDelta += tempDelta;
        }

        var hueAddend = 1;
        var tempStep = absoluteTotalTempDelta / divisions;
        var totalTempDelta = 0.0;
        lastTemp = RelativeTemperature(startHct);
        while (allColors.Count < divisions)
        {
            var hue = MathUtils.SanitizeDegrees(startHue + hueAddend);
            var hct = HctsByHue[hue];
            var temp = RelativeTemperature(hct);
            var tempDelta = Math.Abs(temp - lastTemp);
            totalTempDelta += tempDelta;

            var desiredTotalTempDeltaForIndex = allColors.Count * tempStep;
            var indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
            var indexAddend = 1;
            // Keep adding this hue to the answers until its temperature is
            // insufficient. This ensures consistent behavior when there aren't
            // [divisions] discrete steps between 0 and 360 in hue with [tempStep]
            // delta in temperature between them.
            //
            // For example, white and black have no analogues: there are no other
            // colors at T100/T0. Therefore, they should just be added to the array
            // as answers.
            while (indexSatisfied && allColors.Count < divisions)
            {
                allColors.Add(hct);
                var desiredTotalTempDeltaForIndex1 = (allColors.Count + indexAddend) * tempStep;
                indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex1;
                indexAddend++;
            }

            lastTemp = temp;
            hueAddend++;
            if (hueAddend > 360)
            {
                while (allColors.Count < divisions)
                    allColors.Add(hct);
                break;
            }
        }

        List<Hct> answers = [Input];

        // First, generate analogues from rotating counter-clockwise.
        var increaseHueCount = (int)Math.Floor((count - 1) / 2.0);
        for (var i = 1; i < increaseHueCount + 1; i++)
        {
            var index = 0 - i;
            while (index < 0)
                index = allColors.Count + index;
            if (index >= allColors.Count)
                index = index % allColors.Count;
            answers.Insert(0, allColors[index]);
        }

        // Second, generate analogues from rotating clockwise.
        var decreaseHueCount = count - increaseHueCount - 1;
        for (var i = 1; i < decreaseHueCount + 1; i++)
        {
            var index = i;
            while (index < 0)
                index = allColors.Count + index;
            if (index >= allColors.Count)
                index = index % allColors.Count;
            answers.Add(allColors[index]);
        }

        return answers;
    }

    /// <summary>
    /// Calculates the relative temperature of an HCT color.
    /// </summary>
    /// <param name="hct">The HCT color.</param>
    /// <returns>Temperature value between 0.0 (coldest) and 1.0 (warmest).</returns>
    public double RelativeTemperature(Hct hct)
    {
        var range = TempsByHct[Warmest] - TempsByHct[Coldest];
        var differenceFromColdest = TempsByHct[hct] - TempsByHct[Coldest];
        // Handle when there's no difference in temperature between warmest and
        // coldest: for example, at T100, only one color is available, white.
        if (range == 0.0)
            return 0.5;
        return differenceFromColdest / range;
    }

    /// <summary>
    /// Gets the relative temperature of the input color.
    /// </summary>
    public double InputRelativeTemperature
    {
        get
        {
            if (_inputRelativeTemperature >= 0.0)
                return _inputRelativeTemperature;

            var coldestTemp = TempsByHct[Coldest];

            var range = TempsByHct[Warmest] - coldestTemp;
            var differenceFromColdest = TempsByHct[Input] - coldestTemp;
            var inputRelativeTemp = range == 0.0 ? 0.5 : differenceFromColdest / range;

            _inputRelativeTemperature = inputRelativeTemp;
            return _inputRelativeTemperature;
        }
    }

    public static bool IsBetween(double angle, double a, double b)
    {
        return a < b ? a <= angle && angle <= b : a <= angle || angle <= b;
    }

    public static double RawTemperature(Hct color)
    {
        var lab = ColorUtils.LabFromArgb(color.Argb);
        var hue = MathUtils.SanitizeDegrees(Math.Atan2(lab[2], lab[1]) * 180.0 / Math.PI);
        var chroma = Math.Sqrt(lab[1] * lab[1] + lab[2] * lab[2]);
        var temperature =
            -0.5
            + 0.02
                * Math.Pow(chroma, 1.07)
                * Math.Cos(MathUtils.SanitizeDegrees(hue - 50.0) * Math.PI / 180.0);
        return temperature;
    }
}
