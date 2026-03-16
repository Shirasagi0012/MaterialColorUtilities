using System;
using System.Runtime.CompilerServices;
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

public class MaterialHostStateLeakTests
{
    [AvaloniaFact]
    public void ObservableSubscription_Dispose_ReleasesObserver_FromSourceHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Red));

        var weakObserver = CreateDisposedObserverFromSourceHost(application);

        MaterialColorTestHelper.ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void ObservableSubscription_Dispose_ReleasesObserver_FromApplicationFallback()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Red));

        var weakObserver = CreateDisposedObserverFromApplicationFallback(application);

        MaterialColorTestHelper.ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void ObservableSubscription_Dispose_ReleasesObserver_FromProviderAnchor()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;

        var owner = new Border();
        MaterialColor.SetScheme(owner, new TonalSpotScheme(Colors.Red));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner);

        var weakObserver = CreateDisposedObserverFromProviderAnchor(provider);

        MaterialColorTestHelper.ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void OwnerChanged_UnsubscribesFromOldOwnerSchemeHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Blue));

        var owner1 = new Border();
        MaterialColor.SetScheme(owner1, new TonalSpotScheme(Colors.Red));

        var owner2 = new Border();
        MaterialColor.SetScheme(owner2, new TonalSpotScheme(Colors.Green));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner1);

        var observer = new RecordingColorObserver();
        using var subscription = MaterialColorHelper
            .CreateSysColorObservable(
                new MaterialColorBindingContext(provider, provider, null, null, null),
                SysColorToken.Primary,
                null,
                Colors.Transparent)
            .Subscribe(observer);

        ((IResourceProvider)provider).RemoveOwner(owner1);
        ((IResourceProvider)provider).AddOwner(owner2);

        var countAfterSwap = observer.Values.Count;

        MaterialColor.GetSchemeHost(owner1).Scheme = new TonalSpotScheme(Colors.Yellow);

        Assert.Equal(countAfterSwap, observer.Values.Count);

        MaterialColor.GetSchemeHost(owner2).Scheme = new TonalSpotScheme(Colors.Purple);

        Assert.True(observer.Values.Count > countAfterSwap);
    }

    [AvaloniaFact]
    public void OwnerChanged_UnsubscribesFromOldThemeHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;

        var owner1 = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        MaterialColor.SetScheme(owner1, new TonalSpotScheme(Colors.Red));

        var owner2 = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        MaterialColor.SetScheme(owner2, new TonalSpotScheme(Colors.Red));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner1);

        var observer = new RecordingColorObserver();
        using var subscription = MaterialColorHelper
            .CreateSysColorObservable(
                new MaterialColorBindingContext(provider, provider, null, null, null),
                SysColorToken.Primary,
                null,
                Colors.Transparent)
            .Subscribe(observer);

        ((IResourceProvider)provider).RemoveOwner(owner1);
        ((IResourceProvider)provider).AddOwner(owner2);

        var publishedCountAfterSwap = observer.Values.Count;

        owner1.RequestedThemeVariant = ThemeVariant.Dark;

        Assert.Equal(publishedCountAfterSwap, observer.Values.Count);

        owner2.RequestedThemeVariant = ThemeVariant.Dark;

        Assert.True(observer.Values.Count > publishedCountAfterSwap);
    }

    [AvaloniaFact]
    public void SchemeHostSwap_UnsubscribesFromOldHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        MaterialColor.SetScheme(application, null);

        var source = new Border();
        var oldHost = new MaterialColorScheme(new TonalSpotScheme(Colors.Red));
        var newHost = new MaterialColorScheme(new TonalSpotScheme(Colors.Blue));
        MaterialColor.SetSchemeHost(source, oldHost);

        var observer = new RecordingColorObserver();
        using var subscription = MaterialColorTestHelper
            .CreateSysColorObservable(source, null, SysColorToken.Primary, null, Colors.Transparent)
            .Subscribe(observer);

        Assert.Equal(MaterialColorTestHelper.ResolvePrimary(oldHost, ThemeVariant.Light), observer.Values[^1]);

        MaterialColor.SetSchemeHost(source, newHost);

        Assert.Equal(MaterialColorTestHelper.ResolvePrimary(newHost, ThemeVariant.Light), observer.Values[^1]);
        var publishedCountAfterSwap = observer.Values.Count;

        oldHost.Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Equal(publishedCountAfterSwap, observer.Values.Count);

        newHost.Scheme = new TonalSpotScheme(Colors.Yellow);

        Assert.True(observer.Values.Count > publishedCountAfterSwap);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingColorObserver> CreateDisposedObserverFromSourceHost(Application source)
    {
        var observer = new RecordingColorObserver();
        var subscription = MaterialColorTestHelper
            .CreateSysColorObservable(source, null, SysColorToken.Primary, null, Colors.Transparent)
            .Subscribe(observer);

        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Blue);

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingColorObserver>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingColorObserver> CreateDisposedObserverFromApplicationFallback(Application application)
    {
        var observer = new RecordingColorObserver();
        var subscription = MaterialColorTestHelper
            .CreateSysColorObservable(null, null, SysColorToken.Primary, null, Colors.Transparent)
            .Subscribe(observer);

        MaterialColor.GetSchemeHost(application).Scheme = new TonalSpotScheme(Colors.Blue);
        application.RequestedThemeVariant = ThemeVariant.Dark;

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        MaterialColor.GetSchemeHost(application).Scheme = new TonalSpotScheme(Colors.Green);
        application.RequestedThemeVariant = ThemeVariant.Light;

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingColorObserver>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingColorObserver> CreateDisposedObserverFromProviderAnchor(
        ResourceDictionary provider
    )
    {
        var observer = new RecordingColorObserver();
        var subscription = MaterialColorHelper
            .CreateSysColorObservable(
                new MaterialColorBindingContext(provider, provider, null, null, null),
                SysColorToken.Primary,
                null,
                Colors.Transparent)
            .Subscribe(observer);

        subscription.Dispose();

        return new WeakReference<RecordingColorObserver>(observer);
    }
}
