using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

/// <summary>
/// <see cref="ColorScheme.IsDark" /> decides which half of every scheme a colour comes from, and
/// the argument it is given is nearly always an element's <c>ActualThemeVariant</c>.
/// </summary>
public class ThemeVariantResolutionTests
{
    [Fact]
    public void TheBuiltInVariantsResolveToThemselves()
    {
        Assert.True(ColorScheme.IsDark(ThemeVariant.Dark));
        Assert.False(ColorScheme.IsDark(ThemeVariant.Light));

        // Default means "inherit", and nothing above it said dark.
        Assert.False(ColorScheme.IsDark(ThemeVariant.Default));
    }

    [Fact]
    public void ACustomVariantResolvesThroughItsInheritanceChain()
    {
        var dim = new ThemeVariant("Dim", ThemeVariant.Dark);
        var dimmer = new ThemeVariant("Dimmer", dim);

        Assert.True(ColorScheme.IsDark(dim));
        Assert.True(ColorScheme.IsDark(dimmer));

        // A chain that names neither is light, rather than an error.
        Assert.False(ColorScheme.IsDark(new ThemeVariant("Sepia", null)));
    }

    [Fact]
    public void NoVariantIsNotAnError()
    {
        // ActualThemeVariant is typed non-nullable but registered without a default, so it reads
        // null before an element's first attach and again while it is being detached — and it
        // hands out that null with no nullable warning at the call site. Throwing here turned an
        // ordinary teardown into a crash.
        Assert.False(ColorScheme.IsDark(null));
    }
}
