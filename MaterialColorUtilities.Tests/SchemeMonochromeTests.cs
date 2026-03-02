using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Scheme;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Scheme;

using DynamicColors;

public class SchemeMonochromeTests
{
    private static readonly MaterialDynamicColors Roles = new();
    [Fact]
    public void DarkTheme_MonochromeSpec()
    {
        var scheme = new SchemeMonochrome(
            sourceColorHct: Hct.From(new ArgbColor(0xff0000ff)),
            isDark: true,
            contrastLevel: 0.0);

        Roles.Primary.GetHct(scheme).Tone.AssertCloseTo(100, 1);
        Roles.OnPrimary.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        Roles.PrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(85, 1);
        Roles.OnPrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(0, 1);
        Roles.Secondary.GetHct(scheme).Tone.AssertCloseTo(80, 1);
        Roles.OnSecondary.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        Roles.SecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(30, 1);
        Roles.OnSecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        Roles.Tertiary.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        Roles.OnTertiary.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        Roles.TertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(60, 1);
        Roles.OnTertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(0, 1);
    }

    [Fact]
    public void LightTheme_MonochromeSpec()
    {
        var scheme = new SchemeMonochrome(
            sourceColorHct: Hct.From(new ArgbColor(0xff0000ff)),
            isDark: false,
            contrastLevel: 0.0);

        // TODO: DEBUG Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.
        Roles.Primary.GetHct(scheme).Tone.AssertCloseTo(0, 1);
        Roles.OnPrimary.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        Roles.PrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(25, 1);
        Roles.OnPrimaryContainer.GetHct(scheme).Tone.AssertCloseTo(100, 1);
        Roles.Secondary.GetHct(scheme).Tone.AssertCloseTo(40, 1);
        Roles.OnSecondary.GetHct(scheme).Tone.AssertCloseTo(100, 1);
        Roles.SecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(85, 1);
        Roles.OnSecondaryContainer.GetHct(scheme).Tone.AssertCloseTo(10, 1);
        Roles.Tertiary.GetHct(scheme).Tone.AssertCloseTo(25, 1);
        Roles.OnTertiary.GetHct(scheme).Tone.AssertCloseTo(90, 1);
        Roles.TertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(49, 1);
        Roles.OnTertiaryContainer.GetHct(scheme).Tone.AssertCloseTo(100, 1);
    }
}

