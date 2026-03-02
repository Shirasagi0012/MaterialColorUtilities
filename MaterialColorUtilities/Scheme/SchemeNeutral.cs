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

using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Scheme;

public class SchemeNeutral : DynamicScheme
{
    public SchemeNeutral(Hct sourceColorHct, bool isDark, double contrastLevel)
        : this(
            sourceColorHct,
            isDark,
            contrastLevel,
            DefaultSpecVersion,
            DefaultPlatform
        )
    {
    }

    public SchemeNeutral(
        Hct sourceColorHct,
        bool isDark,
        double contrastLevel,
        ColorSpec.SpecVersion specVersion,
        Platform platform
    )
        : this([sourceColorHct], isDark, contrastLevel, specVersion, platform)
    {
    }

    public SchemeNeutral(IReadOnlyList<Hct> sourceColorHctList, bool isDark, double contrastLevel)
        : this(
            sourceColorHctList,
            isDark,
            contrastLevel,
            DefaultSpecVersion,
            DefaultPlatform
        )
    {
    }

    public SchemeNeutral(
        IReadOnlyList<Hct> sourceColorHctList,
        bool isDark,
        double contrastLevel,
        ColorSpec.SpecVersion specVersion,
        Platform platform
    )
        : base(
            sourceColorHctList,
            Variant.Neutral,
            isDark,
            contrastLevel,
            platform,
            specVersion,
            ColorSpecs.Get(specVersion)
                .GetPrimaryPalette(Variant.Neutral, sourceColorHctList[0], isDark, platform, contrastLevel),
            ColorSpecs.Get(specVersion)
                .GetSecondaryPalette(Variant.Neutral, sourceColorHctList[0], isDark, platform, contrastLevel),
            ColorSpecs.Get(specVersion)
                .GetTertiaryPalette(Variant.Neutral, sourceColorHctList[0], isDark, platform, contrastLevel),
            ColorSpecs.Get(specVersion)
                .GetNeutralPalette(Variant.Neutral, sourceColorHctList[0], isDark, platform, contrastLevel),
            ColorSpecs.Get(specVersion)
                .GetNeutralVariantPalette(Variant.Neutral, sourceColorHctList[0], isDark, platform, contrastLevel),
            ColorSpecs.Get(specVersion)
                .GetErrorPalette(Variant.Neutral, sourceColorHctList[0], isDark, platform, contrastLevel)
        )
    {
    }
}
