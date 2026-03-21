using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorBindingLifecycleTests
{
    [AvaloniaFact]
    public void DetachedButton_CanBeCollectedWhileGlobalSchemeStaysAlive()
    {
        var scheme = new TonalSpotScheme(Colors.Red);
        var weakButton = CreateDetachedButtonReference(scheme);

        Assert.Equal(1, GetSchemeChangedSubscriberCount(scheme));

        ForceFullCollection();

        Assert.False(weakButton.TryGetTarget(out _));

        scheme.Color = Colors.Blue;

        Assert.Equal(0, GetSchemeChangedSubscriberCount(scheme));
    }

    [AvaloniaFact]
    public void ReplacingSchemeInput_UnsubscribesFromOldScheme()
    {
        var target = new Border();
        var oldScheme = new TonalSpotScheme(Colors.Red);
        var newScheme = new TonalSpotScheme(Colors.Blue);
        MaterialColor.SetScheme(target, oldScheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            target);
        target.Bind(Border.BackgroundProperty, binding);

        MaterialColor.SetScheme(target, newScheme);
        var countAfterSwap = Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color;

        oldScheme.Color = Colors.Green;

        Assert.Equal(countAfterSwap, Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);

        newScheme.Color = Colors.Purple;

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(newScheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void DisposingBindingHandle_StopsFurtherUpdates()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(target, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            target);
        var handle = target.Bind(Border.BackgroundProperty, binding);

        handle.Dispose();
        scheme.Color = Colors.Blue;

        Assert.Null(target.Background);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Button> CreateDetachedButtonReference(ColorScheme scheme)
    {
        var host = new StackPanel();
        var button = new Button();
        host.Children.Add(button);

        MaterialColor.SetScheme(button, scheme);

        host.Children.Remove(button);

        return new WeakReference<Button>(button);
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
}
