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

namespace MaterialColorUtilities.Palettes;

/// <summary>
/// A convenience class for retrieving colors that are constant in hue and
/// chroma, but vary in tone.
///
/// This class can be instantiated in two ways:
/// <list type="number">
///     <item><see cref="TonalPalette(double, double)"/> From hue and chroma. (preferred)</item>
///     <item>
///         <see cref="FromList"/> From a fixed-size (<see cref="TonalPalette.CommonSize"/>) list of ints
///         representing ARBG colors. Correctness (constant hue and chroma) of the input
///         is not enforced. <see cref="Get"/> will only return the input colors, corresponding to
///         <see cref="CommonTones"/>. This also initializes the key color to black.
///     </item>
/// </list>
/// </summary>
public class TonalPalette
{
    public static readonly List<int> CommonTones =
    [
        0,
        10,
        20,
        30,
        40,
        50,
        60,
        70,
        80,
        90,
        95,
        99,
        100
    ];

    public static int CommonSize = CommonTones.Count;

    public readonly double Hue;
    public readonly double Chroma;
    public readonly Hct KeyColor;

    /// <summary>
    /// A cache containing keys-value pairs where:
    /// <list type="bullet">
    ///     <item>keys are integers that represent tones, and</item>
    ///     <item>values are colors in ARGB format.</item>
    /// </list>
    /// </summary>
    private readonly Dictionary<int, ArgbColor> _cache = [];

    private readonly bool _isFromCache;

    private TonalPalette(Hct hct)
    {
        KeyColor = hct;
        Hue = hct.Hue;
        Chroma = hct.Chroma;
        _isFromCache = false;
    }

    public TonalPalette(double hue, double chroma)
    {
        Hue = hue;
        Chroma = chroma;
        KeyColor = new KeyColor(hue, chroma).Create();
        _isFromCache = false;
    }

    /// <summary>
    /// Create a Tonal Palette from hue and chroma of hct.
    /// </summary>
    public static TonalPalette FromHct(Hct hct)
    {
        return new TonalPalette(hct);
    }

    private TonalPalette(Dictionary<int, ArgbColor> cache, double hue, double chroma)
    {
        _cache = cache;
        Hue = hue;
        Chroma = chroma;
        KeyColor = new KeyColor(hue, chroma).Create();
        _isFromCache = true;
    }

    /// <summary>
    /// Create colors from a fixed-size list of ARGB color ints.
    ///
    /// Inverse of <see cref="TonalPalette.AsList"/>.
    /// </summary>
    public static TonalPalette FromList(List<ArgbColor> colors)
    {
        if (colors.Count != CommonSize)
            throw new InvalidOperationException(
                "Colors is not equal to CommonSize in TonalPalette."
            );

        Dictionary<int, ArgbColor> cache = [];

        foreach (var (index, toneValue) in CommonTones.Select((value, i) => (i, value)))
            cache[toneValue] = colors[index];

        // Approximately deduces the original hue and chroma that generated this
        // list of colors.
        // Uses the hue and chroma of the provided color with the highest chroma.

        // If the color is too close to white (Tone > 98), its chroma may have been
        // affected by a known issue, so we ignore it.
        // https://github.com/material-foundation/material-color-utilities/issues/140

        var bestHue = 0.0;
        var bestChroma = 0.0;
        foreach (var hct in colors.Select(Hct.From).Where(x => x.Tone <= 98.0))
            if (hct.Chroma > bestChroma)
            {
                bestHue = hct.Hue;
                bestChroma = hct.Chroma;
            }

        return new TonalPalette(cache, bestHue, bestChroma);
    }

    public List<ArgbColor> AsList()
    {
        return CommonTones.Select(Get).ToList();
    }

    public ArgbColor Get(int tone)
    {
        if (_cache.TryGetValue(tone, out var color))
        {
            return color;
        }
        else
        {
            var argb = Hct.From(Hue, Chroma, tone).Argb;
            _cache[tone] = argb;
            return argb;
        }
    }

    public Hct GetHct(double tone)
    {
        if (_cache.TryGetValue((int)tone, out var color))
        {
            return Hct.From(color);
        }
        else
        {
            var argb = Hct.From(Hue, Chroma, tone);
            _cache[(int)tone] = argb.Argb;
            return argb;
        }
    }

    public override string ToString()
    {
        return _isFromCache switch
        {
            true => $"TonalPalette (from cache) {AsList()}",
            false => $"TonalPalette {Hue:F1}°, {Chroma:F1}"
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is TonalPalette other)
        {
            if (_isFromCache != other._isFromCache)
                return false;

            if (_isFromCache)
            {
                // Compare cache contents
                if (_cache.Count != other._cache.Count)
                    return false;
                foreach (var kvp in _cache)
                    if (
                        !other._cache.TryGetValue(kvp.Key, out var otherValue)
                        || !kvp.Value.Equals(otherValue)
                    )
                        return false;
                return true;
            }
            else
            {
                // Compare hue and chroma
                return Math.Abs(Hue - other.Hue) < 0.001 && Math.Abs(Chroma - other.Chroma) < 0.001;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        if (_isFromCache) return _cache.GetHashCode();
        return HashCode.Combine(Hue, Chroma);
    }

    public static bool operator ==(TonalPalette? left, TonalPalette? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(TonalPalette? left, TonalPalette? right)
    {
        return !(left == right);
    }
}