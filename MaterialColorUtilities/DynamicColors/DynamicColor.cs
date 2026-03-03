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

namespace MaterialColorUtilities.DynamicColors;

using HCT;
using Palettes;
using Utils;

/// <summary>
/// A color that adjusts itself based on UI state represented by <see cref="DynamicScheme"/>.
/// </summary>
public sealed class DynamicColor
{
    public readonly string Name;
    public readonly Func<DynamicScheme, TonalPalette> Palette;
    public readonly Func<DynamicScheme, double> Tone;
    public readonly bool IsBackground;
    public readonly Func<DynamicScheme, double>? ChromaMultiplier;
    public readonly Func<DynamicScheme, DynamicColor?>? Background;
    public readonly Func<DynamicScheme, DynamicColor?>? SecondBackground;
    public readonly Func<DynamicScheme, ContrastCurve?>? ContrastCurve;
    public readonly Func<DynamicScheme, ToneDeltaPair?>? ToneDeltaPair;
    public readonly Func<DynamicScheme, double?>? Opacity;

    private const int HctCacheCapacity = 32;

#if NET9_0_OR_GREATER
    private readonly Lock _hctCacheLock = new();
#else
    private readonly object _hctCacheLock = new();
#endif
    private readonly Dictionary<DynamicScheme, LinkedListNode<HctCacheEntry>> _hctCacheMap = new();
    private readonly LinkedList<HctCacheEntry> _hctCacheLruList = new();

    private readonly record struct HctCacheEntry(DynamicScheme Scheme, Hct Value);

    public DynamicColor(
        string name,
        Func<DynamicScheme, TonalPalette> palette,
        Func<DynamicScheme, double> tone,
        bool isBackground,
        Func<DynamicScheme, double>? chromaMultiplier,
        Func<DynamicScheme, DynamicColor?>? background,
        Func<DynamicScheme, DynamicColor?>? secondBackground,
        Func<DynamicScheme, ContrastCurve?>? contrastCurve,
        Func<DynamicScheme, ToneDeltaPair?>? toneDeltaPair,
        Func<DynamicScheme, double?>? opacity
    )
    {
        Name = name;
        Palette = palette;
        Tone = tone;
        IsBackground = isBackground;
        ChromaMultiplier = chromaMultiplier;
        Background = background;
        SecondBackground = secondBackground;
        ContrastCurve = contrastCurve;
        ToneDeltaPair = toneDeltaPair;
        Opacity = opacity;
    }

    public static DynamicColor FromPalette(
        string name,
        Func<DynamicScheme, TonalPalette> palette,
        Func<DynamicScheme, double> tone,
        bool isBackground = false,
        Func<DynamicScheme, DynamicColor?>? background = null,
        Func<DynamicScheme, DynamicColor?>? secondBackground = null,
        Func<DynamicScheme, ContrastCurve?>? contrastCurve = null,
        Func<DynamicScheme, ToneDeltaPair?>? toneDeltaPair = null,
        Func<DynamicScheme, double>? chromaMultiplier = null,
        Func<DynamicScheme, double?>? opacity = null
    )
    {
        return new DynamicColor(
            name,
            palette,
            tone,
            isBackground,
            chromaMultiplier,
            background,
            secondBackground,
            contrastCurve,
            toneDeltaPair,
            opacity
        );
    }

    public ArgbColor GetArgb(DynamicScheme scheme)
    {
        var argb = GetHct(scheme).Argb;
        var opacity = Opacity?.Invoke(scheme);
        if (opacity is null)
            return argb;

        var alpha = Math.Clamp((int)Math.Round(opacity.Value * 255.0), 0, 255);
        return new ArgbColor((argb.Value & 0x00FFFFFF) | (alpha << 24));
    }

    public Hct GetHct(DynamicScheme scheme)
    {
        lock (_hctCacheLock)
        {
            if (_hctCacheMap.TryGetValue(scheme, out var cachedNode))
            {
                _hctCacheLruList.Remove(cachedNode);
                _hctCacheLruList.AddFirst(cachedNode);
                return cachedNode.Value.Value;
            }

            var answer = ColorSpecs.Get(scheme.SpecVersion).GetHct(scheme, this);

            var entryNode = new LinkedListNode<HctCacheEntry>(new HctCacheEntry(scheme, answer));
            _hctCacheLruList.AddFirst(entryNode);
            _hctCacheMap[scheme] = entryNode;

            if (_hctCacheMap.Count > HctCacheCapacity)
            {
                var lruNode = _hctCacheLruList.Last;
                if (lruNode != null)
                {
                    _hctCacheLruList.RemoveLast();
                    _hctCacheMap.Remove(lruNode.Value.Scheme);
                }
            }

            return answer;
        }
    }

    public double GetTone(DynamicScheme scheme)
    {
        return ColorSpecs.Get(scheme.SpecVersion).GetTone(scheme, this);
    }

    public static DynamicColor FromArgb(string name, ArgbColor argb)
    {
        var hct = Hct.From(argb);
        var palette = new TonalPalette(hct);
        return FromPalette(name, _ => palette, _ => hct.Tone);
    }

    public DynamicColor With(
        string? name = null,
        Func<DynamicScheme, TonalPalette>? palette = null,
        Func<DynamicScheme, double>? tone = null,
        bool? isBackground = null,
        Func<DynamicScheme, double>? chromaMultiplier = null,
        Func<DynamicScheme, DynamicColor?>? background = null,
        Func<DynamicScheme, DynamicColor?>? secondBackground = null,
        Func<DynamicScheme, ContrastCurve?>? contrastCurve = null,
        Func<DynamicScheme, ToneDeltaPair?>? toneDeltaPair = null,
        Func<DynamicScheme, double?>? opacity = null
    )
    {
        return new DynamicColor(
            name ?? Name,
            palette ?? Palette,
            tone ?? Tone,
            isBackground ?? IsBackground,
            chromaMultiplier ?? ChromaMultiplier,
            background ?? Background,
            secondBackground ?? SecondBackground,
            contrastCurve ?? ContrastCurve,
            toneDeltaPair ?? ToneDeltaPair,
            opacity ?? Opacity
        );
    }

    public static double ForegroundTone(double bgTone, double ratio)
    {
        var lighterTone = Contrast.Contrast.LighterUnsafe(bgTone, ratio);
        var darkerTone = Contrast.Contrast.DarkerUnsafe(bgTone, ratio);
        var lighterRatio = Contrast.Contrast.RatioOfTones(lighterTone, bgTone);
        var darkerRatio = Contrast.Contrast.RatioOfTones(darkerTone, bgTone);
        var preferLighter = TonePrefersLightForeground(bgTone);

        if (preferLighter)
        {
            var negligibleDifference =
                Math.Abs(lighterRatio - darkerRatio) < 0.1
                && lighterRatio < ratio
                && darkerRatio < ratio;

            return lighterRatio >= ratio || lighterRatio >= darkerRatio || negligibleDifference
                ? lighterTone
                : darkerTone;
        }

        return darkerRatio >= ratio || darkerRatio >= lighterRatio ? darkerTone : lighterTone;
    }

    public static double EnableLightForeground(double tone)
    {
        if (TonePrefersLightForeground(tone) && !ToneAllowsLightForeground(tone))
            return 49.0;

        return tone;
    }

    public static bool TonePrefersLightForeground(double tone)
    {
        return Math.Round(tone) < 60.0;
    }

    public static bool ToneAllowsLightForeground(double tone)
    {
        return Math.Round(tone) <= 49.0;
    }

    public static Func<DynamicScheme, double> GetInitialToneFromBackground(
        Func<DynamicScheme, DynamicColor?>? background
    )
    {
        if (background == null)
            return _ => 50.0;

        return s =>
        {
            var bg = background(s);
            return bg == null ? 50.0 : bg.GetTone(s);
        };
    }

    public DynamicColor ExtendSpecVersion(
        ColorSpec.SpecVersion specVersion,
        DynamicColor extendedColor
    )
    {
        ValidateExtendedColor(specVersion, extendedColor);

        return new DynamicColor(
            Name,
            s => (s.SpecVersion >= specVersion ? extendedColor.Palette : Palette).Invoke(s),
            s => (s.SpecVersion >= specVersion ? extendedColor.Tone : Tone).Invoke(s),
            IsBackground,
            s =>
                (s.SpecVersion >= specVersion
                    ? extendedColor.ChromaMultiplier
                    : ChromaMultiplier
                )?.Invoke(s) ?? 1.0,
            s =>
                (s.SpecVersion >= specVersion ? extendedColor.Background : Background)
                ?.Invoke(s),
            s =>
                (s.SpecVersion >= specVersion
                    ? extendedColor.SecondBackground
                    : SecondBackground
                )?.Invoke(s),
            s =>
                (s.SpecVersion >= specVersion ? extendedColor.ContrastCurve : ContrastCurve)
                ?.Invoke(s),
            s =>
                (s.SpecVersion >= specVersion ? extendedColor.ToneDeltaPair : ToneDeltaPair)
                ?.Invoke(s),
            s => (s.SpecVersion >= specVersion ? extendedColor.Opacity : Opacity)?.Invoke(s)
        );
    }

    private void ValidateExtendedColor(
        ColorSpec.SpecVersion specVersion,
        DynamicColor extendedColor
    )
    {
        if (Name != extendedColor.Name)
            throw new ArgumentException(
                $"Attempting to extend color {Name} with color {extendedColor.Name} of different name for spec version {specVersion}."
            );

        if (IsBackground != extendedColor.IsBackground)
            throw new ArgumentException(
                $"Attempting to extend color {Name} as a {(IsBackground ? "background" : "foreground")} with color {extendedColor.Name} as a {(extendedColor.IsBackground ? "background" : "foreground")} for spec version {specVersion}."
            );
    }
}
