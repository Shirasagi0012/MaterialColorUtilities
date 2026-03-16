using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;

namespace MaterialColorUtilities.Tests.Avalonia.TestUtils;

internal static class MaterialColorTestHelper
{
    internal static Color ResolveSys(
        MaterialColorScheme.MaterialColorSchemeInternal? scheme,
        SysColorToken token,
        ThemeVariant themeVariant
    )
    {
        return scheme?.ResolveSys(token, themeVariant)
               ?? throw new InvalidOperationException("Expected a resolved system color.");
    }

    internal static Color ResolvePrimary(MaterialColorScheme schemeHost, ThemeVariant themeVariant)
    {
        return schemeHost.Internal.ResolveSys(SysColorToken.Primary, themeVariant)
               ?? throw new InvalidOperationException("Expected a primary color.");
    }

    internal static void ForceFullGc()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    internal static IObservable<Color> CreateSysColorObservable(
        AvaloniaObject? source,
        IThemeVariantHost? themeHost,
        SysColorToken token,
        string? customKey,
        Color fallbackColor,
        ThemeVariant? themeVariant = null
    )
    {
        var context = new MaterialColorBindingContext(
            (object?)themeHost ?? source,
            null,
            source,
            themeVariant,
            null
        );

        return MaterialColorHelper.CreateSysColorObservable(context, token, customKey, fallbackColor);
    }
}

internal sealed class RecordingColorObserver : IObserver<Color>
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

internal sealed class TestParentStackProvider(IEnumerable<object> parents) : IAvaloniaXamlIlParentStackProvider
{
    public IEnumerable<object> Parents { get; } = parents;
}
