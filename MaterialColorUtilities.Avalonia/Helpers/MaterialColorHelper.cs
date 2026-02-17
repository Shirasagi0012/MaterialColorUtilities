using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal static class MaterialColorHelper
{
    private static readonly IBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);
    private static readonly Dictionary<uint, IBrush> BrushCache = [];

    private static readonly ClrPropertyInfo SchemeRevisionPropertyInfo =
        new(
            nameof(MaterialColorScheme.Revision),
            target => ((MaterialColorScheme)target).Revision,
            (_, _) => { },
            typeof(int)
        );

    private static readonly ClrPropertyInfo ThemeVariantPropertyInfo =
        new(
            nameof(IThemeVariantHost.ActualThemeVariant),
            target => target is IThemeVariantHost host ? host.ActualThemeVariant : ThemeVariant.Light,
            (_, _) => { },
            typeof(ThemeVariant)
        );

    public static (IProvideValueTarget, IAvaloniaXamlIlParentStackProvider) GetContextServices(
        IServiceProvider services
    )
    {
        if (services.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget target)
            throw new InvalidOperationException($"Service '{nameof(IProvideValueTarget)}' not found.");

        if (services.GetService(typeof(IAvaloniaXamlIlParentStackProvider)) is not IAvaloniaXamlIlParentStackProvider
            parentStack)
            throw new InvalidOperationException($"Service '{nameof(IAvaloniaXamlIlParentStackProvider)}' not found.");

        return (target, parentStack);
    }

    public static IBinding ProvideSysColorBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        Color? fallback = null
    )
    {
        if (RequiresCustomKey(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var fallbackColor = fallback ?? Colors.Transparent;
        var (scheme, themeHost) = ResolveSchemeAndThemeHost(target, parentStack);
        if (scheme is null) return CreateConstantBinding(fallbackColor);

        var normalizedKey = customKey?.Trim();
        return CreateThemeAwareBinding(
            themeHost,
            scheme,
            fallbackColor,
            theme => ResolveSysColor(scheme, token, normalizedKey, theme, fallbackColor)
        );
    }

    public static IBinding ProvideRefColorBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        RefPaletteToken token,
        byte tone,
        Color? fallback = null
    )
    {
        var fallbackColor = fallback ?? Colors.Transparent;
        var (scheme, themeHost) = ResolveSchemeAndThemeHost(target, parentStack);
        if (scheme is null) return CreateConstantBinding(fallbackColor);

        return CreateThemeAwareBinding(
            themeHost,
            scheme,
            fallbackColor,
            theme => ResolveRefColor(scheme, token, tone, theme, fallbackColor)
        );
    }

    public static IBinding ProvideSysBrushBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        IBrush? fallback = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideSysColorBinding(target, parentStack, token, customKey);
        return CreateBrushBinding(colorBinding, fallbackBrush);
    }

    public static IBinding ProvideRefBrushBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        RefPaletteToken token,
        byte tone,
        IBrush? fallback = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideRefColorBinding(target, parentStack, token, tone);
        return CreateBrushBinding(colorBinding, fallbackBrush);
    }

    private static IBinding CreateBrushBinding(IBinding colorBinding, IBrush fallbackBrush)
    {
        return new MultiBinding
        {
            Priority = BindingPriority.Style,
            FallbackValue = fallbackBrush,
            Bindings = { colorBinding },
            Converter = new FuncMultiValueConverter<object?, IBrush>(values =>
            {
                var list = values as IList<object?> ?? values.ToList();
                if (list.Count == 0)
                    return fallbackBrush;

                return ResolveBrushValue(list[0], fallbackBrush);
            })
        };
    }

    private static IBrush ResolveBrushValue(object? value, IBrush fallbackBrush)
    {
        if (value is BindingNotification notification)
            value = notification.Value;

        return value switch
        {
            Color color => GetCachedBrush(color),
            ISolidColorBrush solidBrush => solidBrush,
            IBrush brush => brush,
            _ => fallbackBrush
        };
    }

    private static IBrush GetCachedBrush(Color color)
    {
        var key =
            ((uint)color.A << 24)
            | ((uint)color.R << 16)
            | ((uint)color.G << 8)
            | color.B;

        if (BrushCache.TryGetValue(key, out var brush))
            return brush;

        if (BrushCache.Count > 512)
            BrushCache.Clear();

        brush = new ImmutableSolidColorBrush(color);
        BrushCache[key] = brush;
        return brush;
    }

    private static IBinding CreateThemeAwareBinding(
        IThemeVariantHost? themeHost,
        MaterialColorScheme scheme,
        Color fallbackColor,
        Func<ThemeVariant, Color> resolveColor
    )
    {
        var defaultTheme = themeHost?.ActualThemeVariant ?? ThemeVariant.Light;

        var themeBinding = themeHost is { }
            ? CreateCompiledBinding(themeHost, ThemeVariantPropertyInfo, defaultTheme)
            : CreateConstantBinding(defaultTheme);

        var revisionBinding = CreateCompiledBinding(scheme, SchemeRevisionPropertyInfo, 0);

        return new MultiBinding
        {
            Priority = BindingPriority.Style,
            FallbackValue = fallbackColor,
            Bindings =
            {
                revisionBinding,
                themeBinding
            },
            Converter = new FuncMultiValueConverter<object?, Color>(values =>
            {
                var theme = defaultTheme;
                var list = values as IList<object?> ?? values.ToList();
                if (list.Count > 1 && list[1] is ThemeVariant variant)
                    theme = variant;

                return resolveColor(theme);
            })
        };
    }

    private static IBinding CreateCompiledBinding(object source, ClrPropertyInfo propertyInfo, object fallbackValue)
    {
        return new CompiledBindingExtension
        {
            Mode = BindingMode.OneWay,
            Priority = BindingPriority.Style,
            Source = source,
            FallbackValue = fallbackValue,
            Path = new CompiledBindingPathBuilder()
                .Property(propertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build()
        };
    }

    private static IBinding CreateConstantBinding(object value)
    {
        return new Binding
        {
            Mode = BindingMode.OneTime,
            Priority = BindingPriority.Style,
            Source = value
        };
    }

    private static IEnumerable<object> EnumerateContextObjects(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack
    )
    {
        if (target is { TargetObject: { } targetObject })
            yield return targetObject;

        if (parentStack is { Parents: { } parents })
            foreach (var parent in parents)

                yield return parent;
    }

    public static bool ShouldProvideBrush(IProvideValueTarget provideValueTarget)
    {
        var targetType = provideValueTarget.TargetProperty switch
        {
            AvaloniaProperty avaloniaProperty => avaloniaProperty.PropertyType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            _ => null
        };

        if (targetType is null)
            return true;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(Color))
            return false;

        return true;
    }

    private static (MaterialColorScheme?, IThemeVariantHost?) ResolveSchemeAndThemeHost(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack
    )
    {
        MaterialColorScheme? scheme = null;
        IThemeVariantHost? themeHost = null;
        foreach (var context in EnumerateContextObjects(target, parentStack))
        {
            if (context is AvaloniaObject avaloniaObject && scheme is null)
                scheme = MaterialColor.GetScheme(avaloniaObject);

            if (context is IThemeVariantHost host && themeHost is null) themeHost = host;

            if (scheme is { } && themeHost is { })
                break;
        }

        return (scheme, themeHost);
    }

    internal static bool RequiresCustomKey(SysColorToken token)
    {
        return token is SysColorToken.Custom
            or SysColorToken.OnCustom
            or SysColorToken.CustomContainer
            or SysColorToken.OnCustomContainer;
    }

    private static Color ResolveSysColor(
        MaterialColorScheme scheme,
        SysColorToken token,
        string? customKey,
        ThemeVariant themeVariant,
        Color fallbackColor
    )
    {
        try
        {
            if (RequiresCustomKey(token))
                return scheme.Resolve(customKey!, token, themeVariant) ?? fallbackColor;

            return scheme.Resolve(token, themeVariant) ?? fallbackColor;
        }
        catch
        {
            return fallbackColor;
        }
    }

    private static Color ResolveRefColor(
        MaterialColorScheme scheme,
        RefPaletteToken token,
        byte tone,
        ThemeVariant themeVariant,
        Color fallbackColor
    )
    {
        try
        {
            return scheme.Resolve(token, tone, themeVariant) ?? fallbackColor;
        }
        catch
        {
            return fallbackColor;
        }
    }
}