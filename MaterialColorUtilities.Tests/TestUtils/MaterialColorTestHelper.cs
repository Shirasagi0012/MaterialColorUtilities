using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;
using DesignTokens;
using MaterialColorUtilities.Avalonia;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia.TestUtils;

internal static class MaterialColorTestHelper
{
    internal static ITokenResolver<Color, RefPaletteTokenKey>? GetRefPaletteTokenResolver(AvaloniaObject element)
    {
        return element.GetValue(TokenHost<Color, RefPaletteTokenKey>.ResolverProperty);
    }

    internal static ITokenResolver<Color, SysColorTokenKey>? GetSysColorTokenResolver(AvaloniaObject element)
    {
        return element.GetValue(TokenHost<Color, SysColorTokenKey>.ResolverProperty);
    }

    internal static Color ResolveSys(ColorScheme scheme, SysColorToken token, ThemeVariant themeVariant)
    {
        return new MaterialColorScheme(scheme).ResolveSys(token, themeVariant);
    }

    internal static Color ResolveRef(ColorScheme scheme, RefPaletteToken palette, byte tone)
    {
        return new MaterialColorScheme(scheme).ResolveRef(palette, tone);
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
