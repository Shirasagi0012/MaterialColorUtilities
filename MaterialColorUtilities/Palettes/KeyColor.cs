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

namespace MaterialColorUtilities.Palettes;

public class KeyColor(double hue, double chroma)
{
    public readonly double Hue = hue;
    public readonly double RequestedChroma = chroma;

    readonly private Dictionary<int, double> _chromaCache = [];
    readonly private double _maxChromaValue = 200.0;

    public Hct Create()
    {
        // Pivot around T50 because T50 has the most chroma available, on
        // average. Thus it is most likely to have a direct answer.
        const int pivotTone = 50;
        const int toneStepSize = 1;
        // Epsilon to accept values slightly higher than the requested chroma.
        const double epsilon = 0.01;

        // Binary search to find the tone that can provide a chroma that is closest
        // to the requested chroma.
        var lowerTone = 0;
        var upperTone = 100;
        while (lowerTone < upperTone)
        {
            var midTone = (lowerTone + upperTone) / 2;
            var isAscending = MaxChroma(midTone) < MaxChroma(midTone + toneStepSize);
            var sufficientChroma = MaxChroma(midTone) >= RequestedChroma - epsilon;

            if (sufficientChroma)
            {
                // Either range [lowerTone, midTone] or [midTone, upperTone] has
                // the answer, so search in the range that is closer the pivot tone.
                if (Math.Abs(lowerTone - pivotTone) < Math.Abs(upperTone - pivotTone))
                {
                    upperTone = midTone;
                }
                else
                {
                    if (lowerTone == midTone)
                        return Hct.From(Hue, RequestedChroma, lowerTone);
                    lowerTone = midTone;
                }
            }
            else
            {
                // As there is no sufficient chroma in the midTone, follow the direction
                // to the chroma peak.
                if (isAscending)
                    lowerTone = midTone + toneStepSize;
                else
                    // Keep midTone for potential chroma peak.
                    upperTone = midTone;
            }
        }

        return Hct.From(Hue, RequestedChroma, lowerTone);
    }

    private double MaxChroma(int tone)
    {
        if (_chromaCache.TryGetValue(tone, out var cache))
        {
            return cache;
        }
        else
        {
            var value = Hct.From(Hue, _maxChromaValue, tone).Chroma;
            _chromaCache[tone] = value;
            return value;
        }
    }
}