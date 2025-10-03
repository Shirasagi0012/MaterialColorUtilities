using MaterialColorUtilities.Contrast;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Contrast;

public class ContrastTests
{
    [Fact]
    public void RatioOfTones_OutOfBoundsInput()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.RatioOfTones(-10.0, 110.0);
        result.AssertCloseTo(21.0, 0.001);
    }

    [Fact]
    public void Lighter_ImpossibleRatioErrors()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.Lighter(tone: 90.0, ratio: 10.0);
        result.AssertCloseTo(-1.0, 0.001);
    }

    [Fact]
    public void Lighter_OutOfBoundsInputAboveErrors()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.Lighter(tone: 110.0, ratio: 2.0);
        result.AssertCloseTo(-1.0, 0.001);
    }

    [Fact]
    public void Lighter_OutOfBoundsInputBelowErrors()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.Lighter(tone: -10.0, ratio: 2.0);
        result.AssertCloseTo(-1.0, 0.001);
    }

    [Fact]
    public void LighterUnsafe_ReturnsMaxTone()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.LighterUnsafe(tone: 100.0, ratio: 2.0);
        result.AssertCloseTo(100, 0.001);
    }

    [Fact]
    public void Darker_ImpossibleRatioErrors()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.Darker(tone: 10.0, ratio: 20.0);
        result.AssertCloseTo(-1.0, 0.001);
    }

    [Fact]
    public void Darker_OutOfBoundsInputAboveErrors()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.Darker(tone: 110.0, ratio: 2.0);
        result.AssertCloseTo(-1.0, 0.001);
    }

    [Fact]
    public void Darker_OutOfBoundsInputBelowErrors()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.Darker(tone: -10.0, ratio: 2.0);
        result.AssertCloseTo(-1.0, 0.001);
    }

    [Fact]
    public void DarkerUnsafe_ReturnsMinTone()
    {
        var result = MaterialColorUtilities.Contrast.Contrast.DarkerUnsafe(tone: 0.0, ratio: 2.0);
        result.AssertCloseTo(0.0, 0.001);
    }
}
