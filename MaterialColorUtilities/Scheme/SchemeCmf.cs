// Copyright 2026 Google LLC
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

using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;

namespace MaterialColorUtilities.Scheme;

/// <summary>
/// A dynamic color theme with one or more source colors, used by CMF variant.
/// </summary>
public class SchemeCmf : DynamicScheme
{
    public SchemeCmf(Hct sourceColorHct, bool isDark, double contrastLevel)
        : this(
            sourceColorHct,
            isDark,
            contrastLevel,
            ColorSpec.SpecVersion.Spec2026,
            DefaultPlatform
        )
    {
    }

    public SchemeCmf(
        Hct sourceColorHct,
        bool isDark,
        double contrastLevel,
        ColorSpec.SpecVersion specVersion,
        Platform platform
    )
        : this([sourceColorHct], isDark, contrastLevel, specVersion, platform)
    {
    }

    public SchemeCmf(IReadOnlyList<Hct> sourceColorHctList, bool isDark, double contrastLevel)
        : this(
            sourceColorHctList,
            isDark,
            contrastLevel,
            ColorSpec.SpecVersion.Spec2026,
            DefaultPlatform
        )
    {
    }

    public SchemeCmf(
        IReadOnlyList<Hct> sourceColorHctList,
        bool isDark,
        double contrastLevel,
        ColorSpec.SpecVersion specVersion,
        Platform platform
    )
        : base(
            sourceColorHctList,
            Variant.Cmf,
            isDark,
            contrastLevel,
            platform,
            specVersion,
            new TonalPalette(sourceColorHctList[0].Hue, sourceColorHctList[0].Chroma),
            new TonalPalette(sourceColorHctList[0].Hue, sourceColorHctList[0].Chroma * 0.5),
            GetTertiaryPalette(sourceColorHctList),
            new TonalPalette(sourceColorHctList[0].Hue, sourceColorHctList[0].Chroma * 0.2),
            new TonalPalette(sourceColorHctList[0].Hue, sourceColorHctList[0].Chroma * 0.2),
            new TonalPalette(23.0, Math.Max(sourceColorHctList[0].Chroma, 50.0))
        )
    {
        if (specVersion != ColorSpec.SpecVersion.Spec2026)
            throw new ArgumentException("SchemeCmf can only be used with spec version 2026.");
    }

    private static TonalPalette GetTertiaryPalette(IReadOnlyList<Hct> sourceColorHctList)
    {
        var sourceColorHct = sourceColorHctList[0];
        var secondarySourceColorHct = sourceColorHctList.Count > 1
            ? sourceColorHctList[1]
            : sourceColorHct;

        if (sourceColorHct.Argb == secondarySourceColorHct.Argb)
            return new TonalPalette(sourceColorHct.Hue, sourceColorHct.Chroma * 0.75);

        return new TonalPalette(secondarySourceColorHct.Hue, secondarySourceColorHct.Chroma);
    }
}
