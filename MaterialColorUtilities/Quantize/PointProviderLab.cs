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

using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Quantize;

/// <summary>
/// Provides points in LAB color space for quantization.
/// LAB is a perceptually uniform color space, making it ideal for color clustering.
/// </summary>
public sealed class PointProviderLab : IPointProvider
{
    public Vector3D FromArgb(ArgbColor argb)
    {
        return ColorUtils.LabFromArgb(argb);
    }

    public ArgbColor ToArgb(Vector3D lab)
    {
        return ColorUtils.ArgbFromLab(lab);
    }

    public double Distance(Vector3D one, Vector3D two)
    {
        var diff = one - two;

        // Standard CIE 1976 delta E formula also takes the square root, unneeded
        // here. This method is used by quantization algorithms to compare distance,
        // and the relative ordering is the same, with or without a square root.

        // This relatively minor optimization is helpful because this method is
        // called at least once for each pixel in an image.
        return diff.X * diff.X + diff.Y * diff.Y + diff.Z * diff.Z;
    }
}