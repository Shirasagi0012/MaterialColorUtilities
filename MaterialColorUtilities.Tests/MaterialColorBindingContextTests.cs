using System;
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

public class MaterialColorBindingIntegrationTests
{
    [AvaloniaFact]
    public void CaptureContext_PrefersProviderAnchorAndReadsDictionaryVariant()
    {
        var owner = new Border();
        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Dark;
        var target = new SolidColorBrush();

        var context = MaterialColorHelper.CaptureContext(
            new TestParentStackProvider([provider, owner]),
            target,
            null);

        Assert.Same(provider, context.Anchor);
        Assert.Same(provider, context.ProviderAnchor);
        Assert.Same(target, context.TargetObject);
        Assert.Equal(ThemeVariant.Dark, context.DictionaryThemeVariant);
    }

    [AvaloniaFact]
    public void ProvideSysColorBinding_UsesCapturedProviderOwner()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        MaterialColor.SetScheme(owner, new TonalSpotScheme(Colors.Red));
        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner);

        var observer = new RecordingColorObserver();
        using var subscription = MaterialColorHelper
            .ProvideSysColorBinding(
                new TestParentStackProvider([provider, owner]),
                SysColorToken.Primary,
                targetObject: null)
            .Subscribe(observer);

        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(owner), ThemeVariant.Light),
            observer.Values[^1]);
    }

    [AvaloniaFact]
    public void ProvideSysColorBinding_UsesDictionaryThemeVariantFromParentStack()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Dark
        };
        MaterialColor.SetScheme(owner, new TonalSpotScheme(Colors.Red));

        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Light;
        ((IResourceProvider)provider).AddOwner(owner);

        var observer = new RecordingColorObserver();
        using var subscription = MaterialColorHelper
            .ProvideSysColorBinding(
                new TestParentStackProvider([provider, owner]),
                SysColorToken.Primary,
                targetObject: null)
            .Subscribe(observer);

        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(owner), ThemeVariant.Light),
            observer.Values[^1]);
        Assert.NotEqual(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(owner), ThemeVariant.Dark),
            observer.Values[^1]);
    }

    [AvaloniaFact]
    public void ToBinding_WhenAttachedToControlProperty_UpdatesAndReleasesTargetAfterClear()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        MaterialColor.SetScheme(application, null);

        var source = new Border();
        MaterialColor.SetScheme(source, new TonalSpotScheme(Colors.Red));

        var weakTarget = CreateBoundTargetAndClearBinding(source);

        MaterialColorTestHelper.ForceFullGc();

        Assert.False(weakTarget.TryGetTarget(out _));
    }

    private static WeakReference<Border> CreateBoundTargetAndClearBinding(Border source)
    {
        var target = new Border();
        var binding = MaterialColorTestHelper
            .CreateSysColorObservable(source, null, SysColorToken.Primary, null, Colors.Transparent)
            .Select<Color, IBrush>(static color => new SolidColorBrush(color))
            .ToBinding();

        var bindingHandle = target.Bind(Border.BackgroundProperty, binding);

        Assert.NotNull(target.Background);
        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(source), ThemeVariant.Light),
            Assert.IsType<SolidColorBrush>(target.Background).Color);

        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Blue);

        Assert.Equal(
            MaterialColorTestHelper.ResolvePrimary(MaterialColor.GetSchemeHost(source), ThemeVariant.Light),
            Assert.IsType<SolidColorBrush>(target.Background).Color);

        bindingHandle.Dispose();
        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Null(target.Background);

        return new WeakReference<Border>(target);
    }
}
