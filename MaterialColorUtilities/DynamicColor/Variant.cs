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

namespace MaterialColorUtilities.DynamicColor;

/// <summary>
/// Set of themes supported by Dynamic Color.
/// Instantiate the corresponding scheme subclass to create colors corresponding to the theme.
/// </summary>
public enum Variant
{
    /// <summary>
    /// All colors are grayscale, no chroma.
    /// </summary>
    Monochrome,

    /// <summary>
    /// Close to grayscale, a hint of chroma.
    /// </summary>
    Neutral,

    /// <summary>
    /// Pastel tokens, low chroma palettes (32).
    /// Default Material You theme at 2021 launch.
    /// </summary>
    TonalSpot,

    /// <summary>
    /// Pastel colors, high chroma palettes. (max).
    /// The primary palette's chroma is at maximum.
    /// Use Fidelity instead if tokens should alter their tone to match the palette vibrancy.
    /// </summary>
    Vibrant,

    /// <summary>
    /// Pastel colors, medium chroma palettes.
    /// The primary palette's hue is different from source color, for variety.
    /// </summary>
    Expressive,

    /// <summary>
    /// Almost identical to Fidelity.
    /// Tokens and palettes match source color.
    /// Primary Container is source color, adjusted to ensure contrast with surfaces.
    /// 
    /// Tertiary palette is analogue of source color.
    /// Found by dividing color wheel by 6, then finding the 2 colors adjacent to source.
    /// The one that increases hue is used.
    /// </summary>
    Content,

    /// <summary>
    /// Tokens and palettes match source color.
    /// Primary Container is source color, adjusted to ensure contrast with surfaces.
    /// For example, if source color is black, it is lightened so it doesn't match surfaces in dark mode.
    /// 
    /// Tertiary palette is complement of source color.
    /// </summary>
    Fidelity,

    /// <summary>
    /// A playful theme - the source color's hue does not appear in the theme.
    /// </summary>
    Rainbow,

    /// <summary>
    /// A playful theme - the source color's hue does not appear in the theme.
    /// </summary>
    FruitSalad
}

public static class VariantExtensions
{
#if NET10_0_OR_GREATER
    extension(Variant @this)
    {
        public string Label => @this switch
        {
            Variant.Monochrome => "monochrome",
            Variant.Neutral => "neutral",
            Variant.TonalSpot => "tonal spot",
            Variant.Vibrant => "vibrant",
            Variant.Expressive => "expressive",
            Variant.Content => "content",
            Variant.Fidelity => "fidelity",
            Variant.Rainbow => "rainbow",
            Variant.FruitSalad => "fruit salad",
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
        };

        public string Description => @this switch
        {
            Variant.Monochrome => "All colors are grayscale, no chroma.",
            Variant.Neutral => "Close to grayscale, a hint of chroma.",
            Variant.TonalSpot => "Pastel tokens, low chroma palettes (32).\nDefault Material You theme at 2021 launch.",
            Variant.Vibrant => "Pastel colors, high chroma palettes. (max).\nThe primary palette's chroma is at maximum.\nUse Fidelity instead if tokens should alter their tone to match the palette vibrancy.",
            Variant.Expressive => "Pastel colors, medium chroma palettes.\nThe primary palette's hue is different from source color, for variety.",
            Variant.Content => "Almost identical to Fidelity.\nTokens and palettes match source color.\nPrimary Container is source color, adjusted to ensure contrast with surfaces.\n\nTertiary palette is analogue of source color.\nFound by dividing color wheel by 6, then finding the 2 colors adjacent to source.\nThe one that increases hue is used.",
            Variant.Fidelity => "Tokens and palettes match source color.\nPrimary Container is source color, adjusted to ensure contrast with surfaces.\nFor example, if source color is black, it is lightened so it doesn't match surfaces in dark mode.\n\nTertiary palette is complement of source color.",
            Variant.Rainbow => "A playful theme - the source color's hue does not appear in the theme.",
            Variant.FruitSalad => "A playful theme - the source color's hue does not appear in the theme.",
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
        };
    }
#endif
}