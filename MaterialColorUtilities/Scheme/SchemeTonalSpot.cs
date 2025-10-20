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

using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Scheme;

using DynamicColors;

/// <summary>
/// A Dynamic Color theme with low to medium colorfulness and a Tertiary
/// TonalPalette with a hue related to the source color. The default
/// Material You theme on Android 12 and 13.
/// </summary>
public class SchemeTonalSpot(Hct sourceColorHct, bool isDark, double contrastLevel)
    : DynamicScheme(
        sourceColorHct,
        Variant.TonalSpot,
        isDark,
        contrastLevel,
        new TonalPalette(sourceColorHct.Hue, 36.0),
        new TonalPalette(sourceColorHct.Hue, 16.0),
        new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 60.0), 24.0),
        new TonalPalette(sourceColorHct.Hue, 6.0),
        new TonalPalette(sourceColorHct.Hue, 8.0)
    );