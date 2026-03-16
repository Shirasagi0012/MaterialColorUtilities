using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialHostStateBehaviorTests
{
    [AvaloniaFact]
    public void ResolveHost_PrefersTargetObjectOverProviderOwner()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Blue));

        var owner = new Border();
        MaterialColor.SetScheme(owner, new TonalSpotScheme(Colors.Red));

        var target = new Border();
        MaterialColor.SetScheme(target, new TonalSpotScheme(Colors.Green));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner);

        using var state = new MaterialHostState(
            new MaterialColorBindingContext(provider, provider, target, null, null),
            application);

        Assert.Same(target, state.HostObject);
        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(target), ThemeVariant.Light),
            MaterialColorTestHelper.ResolveSys(state.Scheme, SysColorToken.Primary, state.ThemeVariant));
    }

    [AvaloniaFact]
    public void ResolveHost_RebindsWhenProviderOwnerChanges()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Blue));

        var owner1 = new Border();
        MaterialColor.SetScheme(owner1, new TonalSpotScheme(Colors.Red));

        var owner2 = new Border();
        MaterialColor.SetScheme(owner2, new TonalSpotScheme(Colors.Green));

        var provider = new ResourceDictionary();

        using var state = new MaterialHostState(
            new MaterialColorBindingContext(provider, provider, null, null, null),
            application);

        Assert.Same(application, state.HostObject);
        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(application), ThemeVariant.Light),
            MaterialColorTestHelper.ResolveSys(state.Scheme, SysColorToken.Primary, state.ThemeVariant));

        ((IResourceProvider)provider).AddOwner(owner1);

        Assert.Same(owner1, state.HostObject);
        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(owner1), ThemeVariant.Light),
            MaterialColorTestHelper.ResolveSys(state.Scheme, SysColorToken.Primary, state.ThemeVariant));

        ((IResourceProvider)provider).RemoveOwner(owner1);
        ((IResourceProvider)provider).AddOwner(owner2);

        Assert.Same(owner2, state.HostObject);
        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(owner2), ThemeVariant.Light),
            MaterialColorTestHelper.ResolveSys(state.Scheme, SysColorToken.Primary, state.ThemeVariant));
    }

    [AvaloniaFact]
    public void ThemeVariant_ExplicitOverrideBeatsDictionaryAndActualTheme()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);

        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        MaterialColor.SetScheme(owner, new TonalSpotScheme(Colors.Red));

        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Light;
        ((IResourceProvider)provider).AddOwner(owner);

        using var state = new MaterialHostState(
            new MaterialColorBindingContext(provider, provider, null, ThemeVariant.Dark, ThemeVariant.Light),
            application);

        Assert.Equal(ThemeVariant.Dark, state.ThemeVariant);
        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(owner), ThemeVariant.Dark),
            MaterialColorTestHelper.ResolveSys(state.Scheme, SysColorToken.Primary, state.ThemeVariant));
    }

    [AvaloniaFact]
    public void ThemeVariant_DictionaryOverrideBeatsActualTheme()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);

        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Dark
        };
        MaterialColor.SetScheme(owner, new TonalSpotScheme(Colors.Red));

        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Light;
        ((IResourceProvider)provider).AddOwner(owner);

        using var state = new MaterialHostState(
            new MaterialColorBindingContext(provider, provider, null, null, ThemeVariant.Light),
            application);

        Assert.Equal(ThemeVariant.Light, state.ThemeVariant);
        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(owner), ThemeVariant.Light),
            MaterialColorTestHelper.ResolveSys(state.Scheme, SysColorToken.Primary, state.ThemeVariant));
    }

    [AvaloniaFact]
    public void ThemeVariant_FallsBackToActualThemeAndThenLight()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Dark
        };
        MaterialColor.SetScheme(owner, new TonalSpotScheme(Colors.Red));

        using var stateWithOwner = new MaterialHostState(
            new MaterialColorBindingContext(owner, null, owner, null, null),
            null);

        Assert.Equal(ThemeVariant.Dark, stateWithOwner.ThemeVariant);

        using var stateWithoutHost = new MaterialHostState(
            new MaterialColorBindingContext(null, null, null, null, null),
            null);

        Assert.Equal(ThemeVariant.Light, stateWithoutHost.ThemeVariant);
    }
}
