using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorResourceLifecycleTests
{
    [AvaloniaFact]
    public void DetachedProviderAndControl_AreCollectedWhileSharedSchemeStaysAlive()
    {
        var scheme = new TonalSpotScheme(Colors.Red);
        var (weakControl, weakProvider) = CreateDetachedReferences(scheme);
        Assert.Equal(1, GetSchemeChangedSubscriberCount(scheme));

        ForceFullCollection();
        Assert.False(weakControl.TryGetTarget(out _));
        Assert.False(weakProvider.TryGetTarget(out _));
        scheme.Color = Colors.Blue;
        Assert.Equal(0, GetSchemeChangedSubscriberCount(scheme));
        GC.KeepAlive(scheme);
    }

    [AvaloniaFact]
    public void ReplacingOrClearingScheme_UnsubscribesDeterministically()
    {
        var target = new Border();
        var oldScheme = new TonalSpotScheme(Colors.Red);
        var newScheme = new TonalSpotScheme(Colors.Blue);
        var provider = new MaterialColorResources { Scheme = oldScheme };
        target.Resources.MergedDictionaries.Add(provider);
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        Assert.Equal(1, GetSchemeChangedSubscriberCount(oldScheme));

        provider.Scheme = newScheme;
        Assert.Equal(0, GetSchemeChangedSubscriberCount(oldScheme));
        Assert.Equal(1, GetSchemeChangedSubscriberCount(newScheme));
        var afterSwap = MaterialColorTestHelper.BrushColor(target.Background);
        oldScheme.Color = Colors.Green;
        Assert.Equal(afterSwap, MaterialColorTestHelper.BrushColor(target.Background));
        newScheme.Color = Colors.Purple;
        Assert.Equal(MaterialColorTestHelper.Primary(newScheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));

        provider.Scheme = null;
        Assert.Equal(0, GetSchemeChangedSubscriberCount(newScheme));
        Assert.Null(target.Background);
    }

    [AvaloniaFact]
    public void DisposingDynamicResourceBinding_StopsUpdates()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        target.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        var handle = target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        Assert.NotNull(target.Background);
        handle.Dispose();
        scheme.Color = Colors.Blue;
        Assert.Null(target.Background);
    }

    [AvaloniaFact]
    public void NativeResourceObservable_FollowsInputsThemeAndMissesUntilDisposed()
    {
        var target = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light };
        var scheme = new TonalSpotScheme(Colors.Red);
        var provider = new MaterialColorResources { Scheme = scheme };
        target.Resources.MergedDictionaries.Add(provider);
        var observer = new RecordingObserver();
        var subscription = target.GetResourceObservable(SysColorToken.Primary).Subscribe(observer);
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), Assert.IsType<Color>(observer.Values[^1]));
        scheme.Color = Colors.Blue;
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), Assert.IsType<Color>(observer.Values[^1]));
        target.RequestedThemeVariant = ThemeVariant.Dark;
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), Assert.IsType<Color>(observer.Values[^1]));
        provider.Scheme = null;
        Assert.Same(AvaloniaProperty.UnsetValue, observer.Values[^1]);

        subscription.Dispose();
        var count = observer.Values.Count;
        provider.Scheme = scheme;
        scheme.Color = Colors.Purple;
        target.RequestedThemeVariant = ThemeVariant.Light;
        Assert.Equal(count, observer.Values.Count);
    }

    [AvaloniaFact]
    public void DisposedNativeObservable_DoesNotKeepTargetAlive()
    {
        var scheme = new TonalSpotScheme(Colors.Red);
        var observer = new RecordingObserver();
        var weakTarget = CreateDisposedObservableTarget(scheme, observer);
        ForceFullCollection();
        Assert.False(weakTarget.TryGetTarget(out _));
        var count = observer.Values.Count;
        scheme.Color = Colors.Blue;
        Assert.Equal(count, observer.Values.Count);
        GC.KeepAlive(observer);
        GC.KeepAlive(scheme);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference<Border>, WeakReference<MaterialColorResources>) CreateDetachedReferences(ColorScheme scheme)
    {
        var host = new StackPanel();
        var target = new Border();
        var provider = new MaterialColorResources { Scheme = scheme };
        target.Resources.MergedDictionaries.Add(provider);
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        host.Children.Add(target);
        host.Children.Remove(target);
        return (new WeakReference<Border>(target), new WeakReference<MaterialColorResources>(provider));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Border> CreateDisposedObservableTarget(ColorScheme scheme, RecordingObserver observer)
    {
        var target = new Border();
        target.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        var subscription = target.GetResourceObservable(SysColorToken.Primary).Subscribe(observer);
        subscription.Dispose();
        return new WeakReference<Border>(target);
    }

    private static void ForceFullCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private static int GetSchemeChangedSubscriberCount(ColorScheme scheme)
    {
        var field = typeof(ColorScheme).GetField("SchemeChanged", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Failed to locate ColorScheme.SchemeChanged backing field.");
        return ((EventHandler?)field.GetValue(scheme))?.GetInvocationList().Length ?? 0;
    }

    private sealed class RecordingObserver : IObserver<object?>
    {
        public List<object?> Values { get; } = [];
        public void OnNext(object? value) => Values.Add(value);
        public void OnError(Exception error) => throw error;
        public void OnCompleted() { }
    }
}
