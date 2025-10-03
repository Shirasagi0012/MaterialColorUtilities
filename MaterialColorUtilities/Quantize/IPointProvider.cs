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
/// Provides conversions between ARGB colors and perceptual color space points.
/// Used by k-means clustering algorithms.
/// </summary>
public interface IPointProvider
{
    /// <summary>
    /// Convert an ARGB color to a point in color space.
    /// </summary>
    Vector3D FromArgb(ArgbColor argb);

    /// <summary>
    /// Convert a point in color space back to an ARGB color.
    /// </summary>
    ArgbColor ToArgb(Vector3D point);

    /// <summary>
    /// Calculate the squared distance between two points in color space.
    /// </summary>
    double Distance(Vector3D a, Vector3D b);
}
