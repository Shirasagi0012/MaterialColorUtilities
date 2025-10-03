using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.HCT;

public class HctRoundTripTests
{
    // Note: This test iterates through all 16,777,216 colors (2^24)
    // Estimated test time: 3-4 minutes
    [Fact(Skip = "Costs around 20 sec to finish.")]
    public void HctPreservesOriginalColor()
    {
        for (var argb = 0xFF000000; argb != 0x00000000; argb++)
        {
            var argbColor = new ArgbColor(argb);
            var hct = Hct.From(argbColor);
            var reconstructedArgb = Hct.From(hct.Hue, hct.Chroma, hct.Tone).Argb;

            reconstructedArgb.AssertColorEquals(argbColor);
        }
    }
}
