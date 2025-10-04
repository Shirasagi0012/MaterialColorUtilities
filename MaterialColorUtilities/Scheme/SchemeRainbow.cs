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

using MaterialColorUtilities.DynamicColor;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Scheme;

/// <summary>
/// A playful theme - the source color's hue does not appear in the theme.
/// </summary>
public class SchemeRainbow(Hct sourceColorHct, bool isDark, double contrastLevel)
    : DynamicScheme(
        sourceColorHct,
        Variant.Rainbow,
        isDark,
        contrastLevel,
        new TonalPalette(sourceColorHct.Hue, 48.0),
        new TonalPalette(sourceColorHct.Hue, 16.0),
        new TonalPalette(MathUtils.SanitizeDegrees(sourceColorHct.Hue + 60.0), 24.0),
        new TonalPalette(sourceColorHct.Hue, 0.0),
        new TonalPalette(sourceColorHct.Hue, 0.0)
    );