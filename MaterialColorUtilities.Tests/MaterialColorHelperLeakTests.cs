using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorHelperLeakTests
{
    [AvaloniaFact]
    public void SysColorObservable_Dispose_ReleasesObserverFromSourceAndSchemeHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Red));

        var weakObserver = CreateDisposedObserverFromSource(application);

        ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void SysColorObservable_Dispose_ReleasesObserverFromApplicationFallback()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Red));

        var weakObserver = CreateDisposedObserverFromApplication(application);

        ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void SysColorObservable_Dispose_ReleasesObserverFromThemeHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Red));

        var weakObserver = CreateDisposedObserverFromThemeHost(application);

        ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void SysColorObservable_WithSourceAndApplicationChains_PrefersSource_ThenFallsBackToApplication_AndDisposesCleanly()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        MaterialColor.SetScheme(application, new TonalSpotScheme(Colors.Blue));
        var source = new Border();
        MaterialColor.SetScheme(source, new TonalSpotScheme(Colors.Red));

        var weakObserver = CreateDisposedObserverWithSourceAndApplicationFallback(source, application);

        ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void SysColorObservable_WhenSchemeHostInstanceChanges_UnsubscribesFromOldHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        MaterialColor.SetScheme(application, null);

        var source = new Border();
        var oldHost = new MaterialColorScheme(new TonalSpotScheme(Colors.Red));
        var newHost = new MaterialColorScheme(new TonalSpotScheme(Colors.Blue));
        MaterialColor.SetSchemeHost(source, oldHost);

        var observer = new RecordingObserver();
        var subscription = MaterialColorHelper
            .CreateSysColorObservable(source, null, SysColorToken.Primary, null, Colors.Transparent)
            .Subscribe(observer);

        Assert.Equal(ResolvePrimaryColor(oldHost), observer.Values[^1]);

        MaterialColor.SetSchemeHost(source, newHost);

        Assert.Equal(ResolvePrimaryColor(newHost), observer.Values[^1]);
        var publishedCountAfterSwap = observer.Values.Count;

        oldHost.Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Equal(publishedCountAfterSwap, observer.Values.Count);
        Assert.Equal(ResolvePrimaryColor(newHost), observer.Values[^1]);

        newHost.Scheme = new TonalSpotScheme(Colors.Yellow);

        Assert.Equal(ResolvePrimaryColor(newHost), observer.Values[^1]);
        Assert.True(observer.Values.Count > publishedCountAfterSwap);

        subscription.Dispose();
    }

    [AvaloniaFact]
    public void ToBinding_WhenAttachedToControlProperty_ReleasesTargetAfterBindingIsCleared()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        MaterialColor.SetScheme(application, null);

        var source = new Border();
        MaterialColor.SetScheme(source, new TonalSpotScheme(Colors.Red));

        var weakTarget = CreateBoundTargetAndClearBinding(source);

        ForceFullGc();

        Assert.False(weakTarget.TryGetTarget(out _));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingObserver> CreateDisposedObserverFromSource(Application source)
    {
        var observable = MaterialColorHelper.CreateSysColorObservable(
            source,
            null,
            SysColorToken.Primary,
            null,
            Colors.Transparent
        );
        var observer = new RecordingObserver();
        var subscription = observable.Subscribe(observer);

        Assert.NotEmpty(observer.Values);

        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Blue);

        Assert.True(observer.Values.Count >= 2);

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingObserver>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingObserver> CreateDisposedObserverFromApplication(Application application)
    {
        var observable = MaterialColorHelper.CreateSysColorObservable(
            null,
            null,
            SysColorToken.Primary,
            null,
            Colors.Transparent
        );
        var observer = new RecordingObserver();
        var subscription = observable.Subscribe(observer);

        Assert.NotEmpty(observer.Values);

        MaterialColor.GetSchemeHost(application).Scheme = new TonalSpotScheme(Colors.Blue);

        Assert.True(observer.Values.Count >= 2);

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        MaterialColor.GetSchemeHost(application).Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingObserver>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingObserver> CreateDisposedObserverFromThemeHost(Application themeHost)
    {
        var observable = MaterialColorHelper.CreateSysColorObservable(
            themeHost,
            themeHost,
            SysColorToken.Primary,
            null,
            Colors.Transparent
        );
        var observer = new RecordingObserver();
        var subscription = observable.Subscribe(observer);

        Assert.NotEmpty(observer.Values);

        themeHost.RequestedThemeVariant = ThemeVariant.Dark;

        Assert.True(observer.Values.Count >= 2);

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        themeHost.RequestedThemeVariant = ThemeVariant.Light;

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingObserver>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingObserver> CreateDisposedObserverWithSourceAndApplicationFallback(
        Border source,
        Application application
    )
    {
        var observable = MaterialColorHelper.CreateSysColorObservable(
            source,
            null,
            SysColorToken.Primary,
            null,
            Colors.Transparent
        );
        var observer = new RecordingObserver();
        var subscription = observable.Subscribe(observer);

        Assert.NotEmpty(observer.Values);
        Assert.Equal(ResolvePrimaryColor(MaterialColor.GetSchemeHost(source)), observer.Values[^1]);

        MaterialColor.SetScheme(source, null);

        Assert.Equal(ResolvePrimaryColor(MaterialColor.GetSchemeHost(application)), observer.Values[^1]);
        Assert.True(observer.Values.Count >= 2);

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        MaterialColor.GetSchemeHost(application).Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingObserver>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Border> CreateBoundTargetAndClearBinding(Border source)
    {
        var target = new Border();
        var binding = MaterialColorHelper
            .CreateSysColorObservable(source, null, SysColorToken.Primary, null, Colors.Transparent)
            .Select<Color, IBrush>(static color => new SolidColorBrush(color))
            .ToBinding();

        var bindingHandle = target.Bind(Border.BackgroundProperty, binding);

        Assert.NotNull(target.Background);
        Assert.Equal(
            ResolvePrimaryBrush(MaterialColor.GetSchemeHost(source)),
            Assert.IsType<SolidColorBrush>(target.Background).Color
        );

        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Blue);

        Assert.Equal(
            ResolvePrimaryBrush(MaterialColor.GetSchemeHost(source)),
            Assert.IsType<SolidColorBrush>(target.Background).Color
        );

        bindingHandle.Dispose();
        MaterialColor.GetSchemeHost(source).Scheme = new TonalSpotScheme(Colors.Green);

        Assert.Null(target.Background);

        return new WeakReference<Border>(target);
    }

    private static Color ResolvePrimaryColor(MaterialColorScheme schemeHost)
    {
        return schemeHost.Internal.ResolveSys(SysColorToken.Primary, ThemeVariant.Light)
               ?? throw new InvalidOperationException("Expected a primary color.");
    }

    private static Color ResolvePrimaryBrush(MaterialColorScheme schemeHost)
    {
        return ResolvePrimaryColor(schemeHost);
    }

    private static void ForceFullGc()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed class RecordingObserver : IObserver<Color>
    {
        public List<Color> Values { get; } = [];

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Color value)
        {
            Values.Add(value);
        }
    }
}
