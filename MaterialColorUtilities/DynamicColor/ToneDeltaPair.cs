// Copyright 2023 Google LLC
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
/// Describes the difference in tone between colors.
/// </summary>
public enum TonePolarity
{
    Darker,
    Lighter,
    Nearer,
    Farther
}

/// <summary>
/// Documents a constraint between two DynamicColors, in which their tones must
/// have a certain distance from each other.
///
/// Prefer a DynamicColor with a background, this is for special cases when
/// designers want tonal distance, literally contrast, between two colors that
/// don't have a background / foreground relationship or a contrast guarantee.
/// </summary>
public class ToneDeltaPair
{
    public readonly DynamicColor RoleA;
    public readonly DynamicColor RoleB;
    public readonly double Delta;
    public readonly TonePolarity Polarity;
    public readonly bool StayTogether;

    /// <summary>
    /// Documents a constraint in tone distance between two DynamicColors.
    ///
    /// The polarity is an adjective that describes "A", compared to "B".
    ///
    /// For instance, ToneDeltaPair(A, B, 15, TonePolarity.Darker, stayTogether) states that
    /// A's tone should be at least 15 darker than B's.
    ///
    /// 'Nearer' and 'Farther' describes closeness to the surface roles. For
    /// instance, ToneDeltaPair(A, B, 10, TonePolarity.Nearer, stayTogether) states that A
    /// should be 10 lighter than B in light mode, and 10 darker than B in dark
    /// mode.
    /// </summary>
    /// <param name="roleA">The first role in a pair.</param>
    /// <param name="roleB">The second role in a pair.</param>
    /// <param name="delta">Required difference between tones. Absolute value, negative
    /// values have undefined behavior.</param>
    /// <param name="polarity">The relative relation between tones of roleA and roleB,
    /// as described above.</param>
    /// <param name="stayTogether">Whether these two roles should stay on the same side of
    /// the "awkward zone" (T50-59). This is necessary for certain cases where
    /// one role has two backgrounds.</param>
    public ToneDeltaPair(
        DynamicColor roleA,
        DynamicColor roleB,
        double delta,
        TonePolarity polarity,
        bool stayTogether
    )
    {
        RoleA = roleA;
        RoleB = roleB;
        Delta = delta;
        Polarity = polarity;
        StayTogether = stayTogether;
    }
}