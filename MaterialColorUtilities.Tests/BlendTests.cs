using MaterialColorUtilities.Blend;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Blend;

public class BlendTests
{
    private static readonly ArgbColor Red = new(0xffff0000);
    private static readonly ArgbColor Blue = new(0xff0000ff);
    private static readonly ArgbColor Green = new(0xff00ff00);
    private static readonly ArgbColor Yellow = new(0xffffff00);

    [Fact]
    public void RedToBlue()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Red, Blue);
        answer.AssertColorEquals(new ArgbColor(0xffFB0057));
    }

    [Fact]
    public void RedToGreen()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Red, Green);
        answer.AssertColorEquals(new ArgbColor(0xffD85600));
    }

    [Fact]
    public void RedToYellow()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Red, Yellow);
        answer.AssertColorEquals(new ArgbColor(0xffD85600));
    }

    [Fact]
    public void BlueToGreen()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Blue, Green);
        answer.AssertColorEquals(new ArgbColor(0xff0047A3));
    }

    [Fact]
    public void BlueToRed()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Blue, Red);
        answer.AssertColorEquals(new ArgbColor(0xff5700DC));
    }

    [Fact]
    public void BlueToYellow()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Blue, Yellow);
        answer.AssertColorEquals(new ArgbColor(0xff0047A3));
    }

    [Fact]
    public void GreenToBlue()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Green, Blue);
        answer.AssertColorEquals(new ArgbColor(0xff00FC94));
    }

    [Fact]
    public void GreenToRed()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Green, Red);
        answer.AssertColorEquals(new ArgbColor(0xffB1F000));
    }

    [Fact]
    public void GreenToYellow()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Green, Yellow);
        answer.AssertColorEquals(new ArgbColor(0xffB1F000));
    }

    [Fact]
    public void YellowToBlue()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Yellow, Blue);
        answer.AssertColorEquals(new ArgbColor(0xffEBFFBA));
    }

    [Fact]
    public void YellowToGreen()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Yellow, Green);
        answer.AssertColorEquals(new ArgbColor(0xffEBFFBA));
    }

    [Fact]
    public void YellowToRed()
    {
        var answer = MaterialColorUtilities.Blend.Blend.Harmonize(Yellow, Red);
        answer.AssertColorEquals(new ArgbColor(0xffFFF6E3));
    }
}
