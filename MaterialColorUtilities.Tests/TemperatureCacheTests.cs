using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Temperature;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Temperature;

public class TemperatureCacheTests
{
    // Note: RawTemperature tests are skipped because the method is private

    [Fact]
    public void RawTemperature_Blue()
    {
        var blueTemp = TemperatureCache.RawTemperature(Hct.From(new ArgbColor(0xff0000ff)));
        blueTemp.AssertCloseTo(-1.393, 0.001);
    }

    [Fact]
    public void RawTemperature_Red()
    {
        var redTemp = TemperatureCache.RawTemperature(Hct.From(new ArgbColor(0xffff0000)));
        redTemp.AssertCloseTo(2.351, 0.001);
    }

    [Fact]
    public void RawTemperature_Green()
    {
        var greenTemp = TemperatureCache.RawTemperature(Hct.From(new ArgbColor(0xff00ff00)));
        greenTemp.AssertCloseTo(-0.267, 0.001);
    }

    [Fact]
    public void RawTemperature_White()
    {
        var whiteTemp = TemperatureCache.RawTemperature(Hct.From(new ArgbColor(0xffffffff)));
        whiteTemp.AssertCloseTo(-0.5, 0.001);
    }

    [Fact]
    public void RawTemperature_Black()
    {
        var blackTemp = TemperatureCache.RawTemperature(Hct.From(new ArgbColor(0xff000000)));
        blackTemp.AssertCloseTo(-0.5, 0.001);
    }

    [Fact]
    public void RelativeTemperature_Blue()
    {
        var blueTemp = new TemperatureCache(Hct.From(new ArgbColor(0xff0000ff))).InputRelativeTemperature;
        blueTemp.AssertCloseTo(0.0, 0.001);
    }

    [Fact]
    public void RelativeTemperature_Red()
    {
        var redTemp = new TemperatureCache(Hct.From(new ArgbColor(0xffff0000))).InputRelativeTemperature;
        redTemp.AssertCloseTo(1.0, 0.001);
    }

    [Fact]
    public void RelativeTemperature_Green()
    {
        var greenTemp = new TemperatureCache(Hct.From(new ArgbColor(0xff00ff00))).InputRelativeTemperature;
        greenTemp.AssertCloseTo(0.467, 0.001);
    }

    [Fact]
    public void RelativeTemperature_White()
    {
        var whiteTemp = new TemperatureCache(Hct.From(new ArgbColor(0xffffffff))).InputRelativeTemperature;
        whiteTemp.AssertCloseTo(0.5, 0.001);
    }

    [Fact]
    public void RelativeTemperature_Black()
    {
        var blackTemp = new TemperatureCache(Hct.From(new ArgbColor(0xff000000))).InputRelativeTemperature;
        blackTemp.AssertCloseTo(0.5, 0.001);
    }

    [Fact]
    public void Complement_Blue()
    {
        var blueComplement = new TemperatureCache(Hct.From(new ArgbColor(0xff0000ff))).Complement!.Value;
        blueComplement.Argb.AssertColorEquals(new ArgbColor(0xff9d0002));
    }

    [Fact]
    public void Complement_Red()
    {
        var redComplement = new TemperatureCache(Hct.From(new ArgbColor(0xffff0000))).Complement!.Value;
        redComplement.Argb.AssertColorEquals(new ArgbColor(0xff007bfc));
    }

    [Fact]
    public void Complement_Green()
    {
        var greenComplement = new TemperatureCache(Hct.From(new ArgbColor(0xff00ff00))).Complement!.Value;
        greenComplement.Argb.AssertColorEquals(new ArgbColor(0xffffd2c9));
    }

    [Fact]
    public void Complement_White()
    {
        var whiteComplement = new TemperatureCache(Hct.From(new ArgbColor(0xffffffff))).Complement!.Value;
        whiteComplement.Argb.AssertColorEquals(new ArgbColor(0xffffffff));
    }

    [Fact]
    public void Complement_Black()
    {
        var blackComplement = new TemperatureCache(Hct.From(new ArgbColor(0xff000000))).Complement!.Value;
        blackComplement.Argb.AssertColorEquals(new ArgbColor(0xff000000));
    }

    [Fact]
    public void Analogous_Blue()
    {
        var blueAnalogous = new TemperatureCache(Hct.From(new ArgbColor(0xff0000ff)))
            .Analogous()
            .Select(e => e.Argb)
            .ToList();
        blueAnalogous[0].AssertColorEquals(new ArgbColor(0xff00590c));
        blueAnalogous[1].AssertColorEquals(new ArgbColor(0xff00564e));
        blueAnalogous[2].AssertColorEquals(new ArgbColor(0xff0000ff));
        blueAnalogous[3].AssertColorEquals(new ArgbColor(0xff6700cc));
        blueAnalogous[4].AssertColorEquals(new ArgbColor(0xff81009f));
    }

    [Fact]
    public void Analogous_Red()
    {
        var redAnalogous = new TemperatureCache(Hct.From(new ArgbColor(0xffff0000)))
            .Analogous()
            .Select(e => e.Argb)
            .ToList();
        redAnalogous[0].AssertColorEquals(new ArgbColor(0xfff60082));
        redAnalogous[1].AssertColorEquals(new ArgbColor(0xfffc004c));
        redAnalogous[2].AssertColorEquals(new ArgbColor(0xffff0000));
        redAnalogous[3].AssertColorEquals(new ArgbColor(0xffd95500));
        redAnalogous[4].AssertColorEquals(new ArgbColor(0xffaf7200));
    }

    [Fact]
    public void Analogous_Green()
    {
        var greenAnalogous = new TemperatureCache(Hct.From(new ArgbColor(0xff00ff00)))
            .Analogous()
            .Select(e => e.Argb)
            .ToList();
        greenAnalogous[0].AssertColorEquals(new ArgbColor(0xffcee900));
        greenAnalogous[1].AssertColorEquals(new ArgbColor(0xff92f500));
        greenAnalogous[2].AssertColorEquals(new ArgbColor(0xff00ff00));
        greenAnalogous[3].AssertColorEquals(new ArgbColor(0xff00fd6f));
        greenAnalogous[4].AssertColorEquals(new ArgbColor(0xff00fab3));
    }

    [Fact]
    public void Analogous_Black()
    {
        var blackAnalogous = new TemperatureCache(Hct.From(new ArgbColor(0xff000000)))
            .Analogous()
            .Select(e => e.Argb)
            .ToList();
        blackAnalogous[0].AssertColorEquals(new ArgbColor(0xff000000));
        blackAnalogous[1].AssertColorEquals(new ArgbColor(0xff000000));
        blackAnalogous[2].AssertColorEquals(new ArgbColor(0xff000000));
        blackAnalogous[3].AssertColorEquals(new ArgbColor(0xff000000));
        blackAnalogous[4].AssertColorEquals(new ArgbColor(0xff000000));
    }

    [Fact]
    public void Analogous_White()
    {
        var whiteAnalogous = new TemperatureCache(Hct.From(new ArgbColor(0xffffffff)))
            .Analogous()
            .Select(e => e.Argb)
            .ToList();
        whiteAnalogous[0].AssertColorEquals(new ArgbColor(0xffffffff));
        whiteAnalogous[1].AssertColorEquals(new ArgbColor(0xffffffff));
        whiteAnalogous[2].AssertColorEquals(new ArgbColor(0xffffffff));
        whiteAnalogous[3].AssertColorEquals(new ArgbColor(0xffffffff));
        whiteAnalogous[4].AssertColorEquals(new ArgbColor(0xffffffff));
    }
}