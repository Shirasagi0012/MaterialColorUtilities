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
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Quantize;

/// <summary>
/// Result of quantization operation containing color frequencies and optional pixel-to-cluster mapping.
/// </summary>
public sealed class QuantizerResult(
    IReadOnlyDictionary<ArgbColor, int> colorToCount,
    IReadOnlyDictionary<ArgbColor, ArgbColor>? inputPixelToClusterPixel = null
)
{
    /// <summary>
    /// Map of colors to their occurrence count.
    /// </summary>
    public IReadOnlyDictionary<ArgbColor, int> ColorToCount { get; } = colorToCount;

    /// <summary>
    /// Optional map from input pixel to cluster representative pixel.
    /// </summary>
    public IReadOnlyDictionary<ArgbColor, ArgbColor> InputPixelToClusterPixel { get; } =
        inputPixelToClusterPixel ?? new Dictionary<ArgbColor, ArgbColor>();
}
