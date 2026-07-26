using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia.TestUtils;

internal static class MaterialColorTestHelper
{
    internal static ResolvedColorScheme? GetResolvedScheme(AvaloniaObject element)
    {
        return MaterialColor.GetResolvedScheme(element);
    }

    internal static Color ResolveSys(
        ColorScheme scheme,
        SysColorToken token,
        ThemeVariant themeVariant,
        string? customKey = null
    )
    {
        return new ResolvedColorScheme(scheme).GetColor(token, themeVariant, customKey);
    }

    internal static bool TryResolveSys(
        ColorScheme scheme,
        SysColorToken token,
        ThemeVariant themeVariant,
        string? customKey,
        out Color color
    )
    {
        return new ResolvedColorScheme(scheme)
            .TryGetColor(token, ColorScheme.IsDark(themeVariant), customKey, out color);
    }

    internal static Color ResolveRef(ColorScheme scheme, RefPaletteToken palette, byte tone, string? customKey = null)
    {
        return new ResolvedColorScheme(scheme).GetPaletteColor(palette, tone, customKey);
    }

    internal static bool TryResolveRef(
        ColorScheme scheme,
        RefPaletteToken palette,
        byte tone,
        string? customKey,
        out Color color
    )
    {
        return new ResolvedColorScheme(scheme).TryGetPaletteColor(palette, tone, customKey, out color);
    }

    internal static BindingBase CreateBinding(
        object extension,
        AvaloniaObject targetObject,
        object targetProperty,
        params object[] parents
    )
    {
        var services = new TestServiceProvider(
            new TestProvideValueTarget(targetObject, targetProperty),
            new TestParentStackProvider(parents));

        return Assert.IsAssignableFrom<BindingBase>(extension switch
        {
            MdSysColorExtension sys => sys.ProvideValue(services),
            MdRefPaletteExtension palette => palette.ProvideValue(services),
            _ => throw new ArgumentOutOfRangeException(nameof(extension))
        });
    }
}

internal sealed class TestProvideValueTarget(object targetObject, object targetProperty) : IProvideValueTarget
{
    public object TargetObject { get; } = targetObject;

    public object TargetProperty { get; } = targetProperty;
}

internal sealed class TestParentStackProvider(IEnumerable<object> parents) : IAvaloniaXamlIlParentStackProvider
{
    public IEnumerable<object> Parents { get; } = parents;
}

internal sealed class TestServiceProvider(
    IProvideValueTarget provideValueTarget,
    IAvaloniaXamlIlParentStackProvider parentStackProvider
) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IProvideValueTarget))
            return provideValueTarget;

        if (serviceType == typeof(IAvaloniaXamlIlParentStackProvider))
            return parentStackProvider;

        return null;
    }
}
