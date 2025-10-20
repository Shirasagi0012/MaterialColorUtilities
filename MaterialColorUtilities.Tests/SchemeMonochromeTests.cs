using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Scheme;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Scheme;

using DynamicColors;

public class SchemeMonochromeTests
{
    [Fact]
    public void DarkTheme_MonochromeSpec()
    {
        var scheme = new SchemeMonochrome(
            sourceColorHct: Hct.From(new ArgbColor(0xff0000ff)),
            isDark: true,
            contrastLevel: 0.0);

        MaterialDynamicColors.Primary.GetHct(scheme).Tone.AssertCloseTo(100, 1);
        MaterialDynamicColors.OnPrimary.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        MaterialDynamicColors.PrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(85, 1);
        MaterialDynamicColors.OnPrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(0, 1);
        MaterialDynamicColors.Secondary.GetHct(scheme).Tone.AssertCloseTo(80, 1);
        MaterialDynamicColors.OnSecondary.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        MaterialDynamicColors.SecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(30, 1);
        MaterialDynamicColors.OnSecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        MaterialDynamicColors.Tertiary.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        MaterialDynamicColors.OnTertiary.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        MaterialDynamicColors.TertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(60, 1);
        MaterialDynamicColors.OnTertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(0, 1);
    }

    [Fact]
    public void LightTheme_MonochromeSpec()
    {
        var scheme = new SchemeMonochrome(
            sourceColorHct: Hct.From(new ArgbColor(0xff0000ff)),
            isDark: false,
            contrastLevel: 0.0);

        // TODO: DEBUG Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.
        MaterialDynamicColors.Primary.GetHct(scheme).Tone.AssertCloseTo(0, 1);
        MaterialDynamicColors.OnPrimary.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        MaterialDynamicColors.PrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(25, 1);
        MaterialDynamicColors.OnPrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(100, 1);
        MaterialDynamicColors.Secondary.GetHct(scheme).Tone.AssertCloseTo(40, 1);
        MaterialDynamicColors.OnSecondary.GetHct(scheme).Tone.AssertCloseTo(100, 1);
        MaterialDynamicColors.SecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(85, 1);
        MaterialDynamicColors.OnSecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        MaterialDynamicColors.Tertiary.GetHct(scheme).Tone.AssertCloseTo(25, 1);
        MaterialDynamicColors.OnTertiary.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        MaterialDynamicColors.TertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(49, 1);
        MaterialDynamicColors.OnTertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(100, 1);
    }
}
