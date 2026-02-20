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

namespace MaterialColorUtilities.DynamicColors;

using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

/// <summary>
/// A color that adjusts itself based on UI state provided by DynamicScheme.
///
/// This color automatically adjusts to accommodate a desired contrast level, or
/// other adjustments such as differing in light mode versus dark mode, or what
/// the theme is, or what the color that produced the theme is, etc.
///
/// Colors without backgrounds do not change tone when contrast changes. Colors
/// with backgrounds become closer to their background as contrast lowers, and
/// further when contrast increases.
///
/// Prefer the static factory methods. They provide a much more simple interface,
/// such as requiring just a palette and tone function.
/// </summary>
public class DynamicColor
{
    public readonly string Name;
    public readonly Func<DynamicScheme, TonalPalette> Palette;
    public readonly Func<DynamicScheme, double> Tone;
    public readonly bool IsBackground;
    public readonly Func<DynamicScheme, DynamicColor>? Background;
    public readonly Func<DynamicScheme, DynamicColor>? SecondBackground;
    public readonly ContrastCurve? ContrastCurve;
    public readonly Func<DynamicScheme, ToneDeltaPair>? ToneDeltaPair;

    readonly private Dictionary<DynamicScheme, Hct> _hctCache = new();

    /// <summary>
    /// The base constructor for DynamicColor.
    ///
    /// Ultimately, each component necessary for calculating a color, adjusting it
    /// for a desired contrast level, and ensuring it has a certain lightness/tone
    /// difference from another color, is provided by a function that takes a
    /// DynamicScheme and returns a value. This ensures ultimate flexibility, any
    /// desired behavior of a color for any design system.
    /// </summary>
    public DynamicColor(
        string name,
        Func<DynamicScheme, TonalPalette> palette,
        Func<DynamicScheme, double> tone,
        bool isBackground,
        Func<DynamicScheme, DynamicColor>? background,
        Func<DynamicScheme, DynamicColor>? secondBackground,
        ContrastCurve? contrastCurve,
        Func<DynamicScheme, ToneDeltaPair>? toneDeltaPair
    )
    {
        Name = name;
        Palette = palette;
        Tone = tone;
        IsBackground = isBackground;
        Background = background;
        SecondBackground = secondBackground;
        ContrastCurve = contrastCurve;
        ToneDeltaPair = toneDeltaPair;
    }

    /// <summary>
    /// A convenience constructor for DynamicColor.
    ///
    /// All parameters other than palette and tone have defaults.
    /// </summary>
    public static DynamicColor FromPalette(
        string name,
        Func<DynamicScheme, TonalPalette> palette,
        Func<DynamicScheme, double> tone,
        bool isBackground = false,
        Func<DynamicScheme, DynamicColor>? background = null,
        Func<DynamicScheme, DynamicColor>? secondBackground = null,
        ContrastCurve? contrastCurve = null,
        Func<DynamicScheme, ToneDeltaPair>? toneDeltaPair = null
    )
    {
        return new DynamicColor(
            name,
            palette,
            tone,
            isBackground,
            background,
            secondBackground,
            contrastCurve,
            toneDeltaPair
        );
    }

    /// <summary>
    /// Return an ARGB integer (i.e. a hex code).
    /// </summary>
    /// <param name="scheme">Defines the conditions of the user interface, for example,
    /// whether or not it is dark mode or light mode, and what the desired
    /// contrast level is.</param>
    public ArgbColor GetArgb(DynamicScheme scheme)
    {
        return GetHct(scheme).Argb;
    }

    /// <summary>
    /// Return a color, expressed in the HCT color space, that this
    /// DynamicColor is under the conditions in scheme.
    /// </summary>
    /// <param name="scheme">Defines the conditions of the user interface, for example,
    /// whether or not it is dark mode or light mode, and what the desired
    /// contrast level is.</param>
    public Hct GetHct(DynamicScheme scheme)
    {
        if (_hctCache.TryGetValue(scheme, out var cachedAnswer))
            return cachedAnswer;

        var tone = GetTone(scheme);
        var answer = Palette(scheme).GetHct(tone);

        if (_hctCache.Count > 4)
            _hctCache.Clear();

        _hctCache[scheme] = answer;
        return answer;
    }

    /// <summary>
    /// Return a tone, T in the HCT color space, that this DynamicColor is under
    /// the conditions in scheme.
    /// </summary>
    /// <param name="scheme">Defines the conditions of the user interface, for example,
    /// whether or not it is dark mode or light mode, and what the desired
    /// contrast level is.</param>
    public double GetTone(DynamicScheme scheme)
    {
        var decreasingContrast = scheme.ContrastLevel < 0;

        // Case 1: dual foreground, pair of colors with delta constraint.
        if (ToneDeltaPair != null)
        {
            var pair = ToneDeltaPair(scheme);
            var roleA = pair.RoleA;
            var roleB = pair.RoleB;
            var delta = pair.Delta;
            var polarity = pair.Polarity;
            var stayTogether = pair.StayTogether;

            var bg = Background!(scheme);
            var bgTone = bg.GetTone(scheme);

            var aIsNearer =
                polarity == TonePolarity.Nearer
                || (polarity == TonePolarity.Lighter && !scheme.IsDark)
                || (polarity == TonePolarity.Darker && scheme.IsDark);
            var nearer = aIsNearer ? roleA : roleB;
            var farther = aIsNearer ? roleB : roleA;
            var amNearer = Name == nearer.Name;
            var expansionDir = scheme.IsDark ? 1 : -1;

            // 1st round: solve to min, each
            var nContrast = nearer.ContrastCurve!.Get(scheme.ContrastLevel);
            var fContrast = farther.ContrastCurve!.Get(scheme.ContrastLevel);

            // If a color is good enough, it is not adjusted.
            // Initial and adjusted tones for `nearer`
            var nInitialTone = nearer.Tone(scheme);
            var nTone =
                Contrast.Contrast.RatioOfTones(bgTone, nInitialTone) >= nContrast
                    ? nInitialTone
                    : ForegroundTone(bgTone, nContrast);

            // Initial and adjusted tones for `farther`
            var fInitialTone = farther.Tone(scheme);
            var fTone =
                Contrast.Contrast.RatioOfTones(bgTone, fInitialTone) >= fContrast
                    ? fInitialTone
                    : ForegroundTone(bgTone, fContrast);

            if (decreasingContrast)
            {
                // If decreasing contrast, adjust color to the "bare minimum"
                // that satisfies contrast.
                nTone = ForegroundTone(bgTone, nContrast);
                fTone = ForegroundTone(bgTone, fContrast);
            }

            if ((fTone - nTone) * expansionDir >= delta)
            {
                // Good! Tones satisfy the constraint; no change needed.
            }
            else
            {
                // 2nd round: expand farther to match delta.
                fTone = Math.Clamp(nTone + delta * expansionDir, 0, 100);
                if ((fTone - nTone) * expansionDir >= delta)
                {
                    // Good! Tones now satisfy the constraint; no change needed.
                }
                else
                {
                    // 3rd round: contract nearer to match delta.
                    nTone = Math.Clamp(fTone - delta * expansionDir, 0, 100);
                }
            }

            // Avoids the 50-59 awkward zone.
            if (nTone is >= 50 and < 60)
            {
                // If `nearer` is in the awkward zone, move it away, together with
                // `farther`.
                if (expansionDir > 0)
                {
                    nTone = 60;
                    fTone = Math.Max(fTone, nTone + delta * expansionDir);
                }
                else
                {
                    nTone = 49;
                    fTone = Math.Min(fTone, nTone + delta * expansionDir);
                }
            }
            else if (fTone is >= 50 and < 60)
            {
                if (stayTogether)
                {
                    // Fixes both, to avoid two colors on opposite sides of the "awkward
                    // zone".
                    if (expansionDir > 0)
                    {
                        nTone = 60;
                        fTone = Math.Max(fTone, nTone + delta * expansionDir);
                    }
                    else
                    {
                        nTone = 49;
                        fTone = Math.Min(fTone, nTone + delta * expansionDir);
                    }
                }
                else
                {
                    // Not required to stay together; fixes just one.
                    fTone = expansionDir > 0 ? 60 : 49;
                }
            }

            // Returns `nTone` if this color is `nearer`, otherwise `fTone`.
            return amNearer ? nTone : fTone;
        }
        else
        {
            // Case 2: No contrast pair; just solve for itself.
            var answer = Tone(scheme);

            if (Background == null)
                return answer; // No adjustment for colors with no background.

            var bgTone = Background(scheme).GetTone(scheme);

            var desiredRatio = ContrastCurve!.Get(scheme.ContrastLevel);

            if (Contrast.Contrast.RatioOfTones(bgTone, answer) >= desiredRatio)
            {
                // Don't "improve" what's good enough.
            }
            else
            {
                // Rough improvement.
                answer = ForegroundTone(bgTone, desiredRatio);
            }

            if (decreasingContrast)
                answer = ForegroundTone(bgTone, desiredRatio);

            if (IsBackground && answer is >= 50 and < 60)
                // Must adjust
                answer = Contrast.Contrast.RatioOfTones(49, bgTone) >= desiredRatio ? 49 : 60;

            if (SecondBackground != null)
            {
                // Case 3: Adjust for dual backgrounds.

                var bgTone1 = Background(scheme).GetTone(scheme);
                var bgTone2 = SecondBackground(scheme).GetTone(scheme);

                var upper = Math.Max(bgTone1, bgTone2);
                var lower = Math.Min(bgTone1, bgTone2);

                if (
                    Contrast.Contrast.RatioOfTones(upper, answer) >= desiredRatio
                    && Contrast.Contrast.RatioOfTones(lower, answer) >= desiredRatio
                )
                    return answer;

                // The darkest light tone that satisfies the desired ratio,
                // or -1 if such ratio cannot be reached.
                var lightOption = Contrast.Contrast.Lighter(upper, desiredRatio);

                // The lightest dark tone that satisfies the desired ratio,
                // or -1 if such ratio cannot be reached.
                var darkOption = Contrast.Contrast.Darker(lower, desiredRatio);

                // Tones suitable for the foreground.
                var availables = new List<double>();
                if (lightOption != -1)
                    availables.Add(lightOption);
                if (darkOption != -1)
                    availables.Add(darkOption);

                var prefersLight =
                    TonePrefersLightForeground(bgTone1) || TonePrefersLightForeground(bgTone2);

                if (prefersLight)
                    return lightOption < 0 ? 100 : lightOption;

                if (availables.Count == 1)
                    return availables[0];

                return darkOption < 0 ? 0 : darkOption;
            }

            return answer;
        }
    }

    /// <summary>
    /// Given a background tone, find a foreground tone, while ensuring they reach
    /// a contrast ratio that is as close to ratio as possible.
    /// </summary>
    /// <param name="bgTone">Tone in HCT. Range is 0 to 100, undefined behavior when it falls
    /// outside that range.</param>
    /// <param name="ratio">The contrast ratio desired between bgTone and the return value.</param>
    public static double ForegroundTone(double bgTone, double ratio)
    {
        var lighterTone = Contrast.Contrast.LighterUnsafe(bgTone, ratio);
        var darkerTone = Contrast.Contrast.DarkerUnsafe(bgTone, ratio);
        var lighterRatio = Contrast.Contrast.RatioOfTones(lighterTone, bgTone);
        var darkerRatio = Contrast.Contrast.RatioOfTones(darkerTone, bgTone);
        var preferLighter = TonePrefersLightForeground(bgTone);

        if (preferLighter)
        {
            // This handles an edge case where the initial contrast ratio is high
            // (ex. 13.0), and the ratio passed to the function is that high ratio,
            // and both the lighter and darker ratio fails to pass that ratio.
            //
            // This was observed with Tonal Spot's On Primary Container turning black
            // momentarily between high and max contrast in light mode.
            // PC's standard tone was T90, OPC's was T10, it was light mode, and the
            // contrast value was 0.6568521221032331.
            var negligibleDifference =
                Math.Abs(lighterRatio - darkerRatio) < 0.1
                && lighterRatio < ratio
                && darkerRatio < ratio;

            return lighterRatio >= ratio || lighterRatio >= darkerRatio || negligibleDifference
                ? lighterTone
                : darkerTone;
        }
        else
        {
            return darkerRatio >= ratio || darkerRatio >= lighterRatio ? darkerTone : lighterTone;
        }
    }

    /// <summary>
    /// Adjust a tone such that white has 4.5 contrast, if the tone is
    /// reasonably close to supporting it.
    /// </summary>
    public static double EnableLightForeground(double tone)
    {
        if (TonePrefersLightForeground(tone) && !ToneAllowsLightForeground(tone))
            return 49.0;
        return tone;
    }

    /// <summary>
    /// Returns whether tone prefers a light foreground.
    ///
    /// People prefer white foregrounds on ~T60-70. Observed over time, and also
    /// by Andrew Somers during research for APCA.
    ///
    /// T60 used as to create the smallest discontinuity possible when skipping
    /// down to T49 in order to ensure light foregrounds.
    ///
    /// Since `tertiaryContainer` in dark monochrome scheme requires a tone of
    /// 60, it should not be adjusted. Therefore, 60 is excluded here.
    /// </summary>
    public static bool TonePrefersLightForeground(double tone)
    {
        return Math.Round(tone, MidpointRounding.AwayFromZero) < 60;
    }

    /// <summary>
    /// Returns whether tone can reach a contrast ratio of 4.5 with a lighter color.
    /// </summary>
    public static bool ToneAllowsLightForeground(double tone)
    {
        return Math.Round(tone, MidpointRounding.AwayFromZero) <= 49;
    }
}
