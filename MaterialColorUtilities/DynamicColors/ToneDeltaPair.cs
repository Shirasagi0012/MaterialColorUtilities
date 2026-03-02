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

namespace MaterialColorUtilities.DynamicColors;

/// <summary>
/// Describes the relationship in tone between two colors.
/// </summary>
public enum TonePolarity
{
    Darker,
    Lighter,
    RelativeDarker,
    RelativeLighter,

    [Obsolete("Use ToneDeltaPair.DeltaConstraint with RelativeDarker/RelativeLighter instead.")]
    Nearer,

    [Obsolete("Use ToneDeltaPair.DeltaConstraint with RelativeDarker/RelativeLighter instead.")]
    Farther,
}

/// <summary>
/// Documents a constraint between two DynamicColors, in which their tones must
/// have a certain distance from each other.
/// </summary>
public sealed class ToneDeltaPair
{
    public enum DeltaConstraint
    {
        Exact,
        Nearer,
        Farther,
    }

    public DynamicColor RoleA { get; }
    public DynamicColor RoleB { get; }
    public double Delta { get; }
    public TonePolarity Polarity { get; }
    public bool StayTogether { get; }
    public DeltaConstraint Constraint { get; }

    /// <summary>
    /// Legacy constructor shape kept for compatibility with existing token definitions.
    /// </summary>
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
        Constraint = DeltaConstraint.Exact;
    }

    /// <summary>
    /// Constructor using explicit delta-constraint behavior.
    /// </summary>
    public ToneDeltaPair(
        DynamicColor roleA,
        DynamicColor roleB,
        double delta,
        TonePolarity polarity,
        DeltaConstraint constraint
    )
    {
        RoleA = roleA;
        RoleB = roleB;
        Delta = delta;
        Polarity = polarity;
        StayTogether = true;
        Constraint = constraint;
    }
}
