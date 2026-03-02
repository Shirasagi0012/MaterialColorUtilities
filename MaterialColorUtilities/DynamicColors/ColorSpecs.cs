// Copyright 2025 Google LLC
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

/// <summary>
/// Resolves the color spec implementation for a given spec version.
/// </summary>
public static class ColorSpecs
{
    private static readonly ColorSpec Spec2021 = new ColorSpec2021();
    private static readonly ColorSpec Spec2025 = new ColorSpec2025();
    private static readonly ColorSpec Spec2026 = new ColorSpec2026();

    public static ColorSpec Get(ColorSpec.SpecVersion specVersion = ColorSpec.SpecVersion.Spec2021, bool isExtendedFidelity = false)
    {
        return specVersion switch
        {
            ColorSpec.SpecVersion.Spec2025 => Spec2025,
            ColorSpec.SpecVersion.Spec2026 => Spec2026,
            _ => Spec2021,
        };
    }
}
