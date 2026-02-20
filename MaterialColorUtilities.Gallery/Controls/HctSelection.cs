using System;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Gallery.Controls;

public readonly record struct HctSelection(double Hue, double Chroma, double Tone)
{
    public HctSelection Normalize()
    {
        return new HctSelection(
            Math.Clamp(Hue, 0.0, 359.0),
            Math.Max(0.0, Chroma),
            Math.Clamp(Tone, 0.0, 100.0));
    }

    public Hct ToHct()
    {
        var normalized = Normalize();
        return Hct.From(normalized.Hue, normalized.Chroma, normalized.Tone);
    }

    public static HctSelection FromHct(Hct hct)
    {
        return new HctSelection(hct.Hue, hct.Chroma, hct.Tone).Normalize();
    }
}
