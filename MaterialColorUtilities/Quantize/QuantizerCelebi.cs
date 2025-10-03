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

using System.Collections.Generic;
using System.Linq;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Quantize;

/// <summary>
/// Celebi color quantization algorithm. Combines Wu quantization for initial seeding
/// with weighted k-means for refinement.
/// </summary>
public sealed class QuantizerCelebi : IQuantizer
{
    public async Task<QuantizerResult> QuantizeAsync(List<ArgbColor> pixels, int maxColors)
    {
        return await QuantizeAsync(pixels, maxColors, false);
    }

    public async Task<QuantizerResult> QuantizeAsync(
        List<ArgbColor> pixels,
        int maxColors,
        bool returnInputPixelToClusterPixel
    )
    {
        var wu = new QuantizerWu();
        var wuResult = await wu.QuantizeAsync(pixels, maxColors);
        var wsmeansResult = QuantizerWsmeans.Quantize(
            pixels,
            maxColors,
            wuResult.ColorToCount.Keys.ToList(),
            new PointProviderLab(),
            returnInputPixelToClusterPixel: returnInputPixelToClusterPixel
        );
        return wsmeansResult;
    }
}
