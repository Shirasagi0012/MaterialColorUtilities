using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MaterialColorUtilities.Avalonia.Internal;

internal static class MaterialColorRuntime
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

    public static IBinding ProvideSysColorBinding(
        IServiceProvider serviceProvider,
        SysColorToken token,
        string? customKey = null,
        Color? fallback = null
    )
    {
        if (RequiresCustomKey(token) && string.IsNullOrWhiteSpace(customKey))
        {
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");
        }

        var fallbackColor = fallback ?? Colors.Transparent;
        var scheme = ResolveScheme(serviceProvider);
        if (scheme is null)
        {
            return CreateConstantBinding(fallbackColor);
        }

        var normalizedKey = customKey?.Trim();
        return CreateThemeAwareBinding(
            serviceProvider,
            scheme,
            fallbackColor,
            theme => ResolveSysColor(scheme, token, normalizedKey, theme, fallbackColor)
        );
    }

    public static IBinding ProvideRefColorBinding(
        IServiceProvider serviceProvider,
        RefPaletteToken token,
        byte tone,
        Color? fallback = null
    )
    {
        if (tone > 100)
            throw new ArgumentOutOfRangeException(nameof(tone), "Tone must be in range 0..100.");

        var fallbackColor = fallback ?? Colors.Transparent;
        var scheme = ResolveScheme(serviceProvider);
        if (scheme is null)
        {
            return CreateConstantBinding(fallbackColor);
        }

        return CreateThemeAwareBinding(
            serviceProvider,
            scheme,
            fallbackColor,
            theme => ResolveRefColor(scheme, token, tone, theme, fallbackColor)
        );
    }

    public static IBinding ProvideSysBrushBinding(
        IServiceProvider serviceProvider,
        SysColorToken token,
        string? customKey = null,
        IBrush? fallback = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideSysColorBinding(serviceProvider, token, customKey);
        return CreateBrushBinding(colorBinding, fallbackBrush);
    }

    public static IBinding ProvideRefBrushBinding(
        IServiceProvider serviceProvider,
        RefPaletteToken token,
        byte tone,
        IBrush? fallback = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideRefColorBinding(serviceProvider, token, tone);
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
        IServiceProvider serviceProvider,
        MaterialColorScheme scheme,
        Color fallbackColor,
        Func<ThemeVariant, Color> resolveColor
    )
    {
        var themeHost = ResolveThemeHost(serviceProvider);
        var defaultTheme = themeHost?.ActualThemeVariant ?? ThemeVariant.Light;

        var themeBinding = themeHost is not null
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
                .Build(),
        };
    }

    private static IBinding CreateConstantBinding(object value)
    {
        return new Binding
        {
            Mode = BindingMode.OneTime,
            Priority = BindingPriority.Style,
            Source = value,
        };
    }

    private static IEnumerable<object> EnumerateContextObjects(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget { TargetObject: { } targetObject })
            yield return targetObject;

        if (serviceProvider.GetService(typeof(IAvaloniaXamlIlParentStackProvider)) is
            IAvaloniaXamlIlEagerParentStackProvider { Parents: { } parents })
        {
            foreach (var parent in parents)
            {
                if (parent is not null)
                    yield return parent;
            }
        }
    }

    private static MaterialColorScheme? ResolveScheme(IServiceProvider serviceProvider)
    {
        foreach (var context in EnumerateContextObjects(serviceProvider))
        {
            if (context is not AvaloniaObject avaloniaObject)
                continue;

            var scheme = MaterialColor.GetScheme(avaloniaObject);
            if (scheme is not null)
                return scheme;
        }

        return null;
    }

    private static IThemeVariantHost? ResolveThemeHost(IServiceProvider serviceProvider)
    {
        foreach (var context in EnumerateContextObjects(serviceProvider))
        {
            if (context is IThemeVariantHost themeHost)
                return themeHost;
        }

        return null;
    }

    private static bool RequiresCustomKey(SysColorToken token) =>
        token is SysColorToken.Custom
            or SysColorToken.OnCustom
            or SysColorToken.CustomContainer
            or SysColorToken.OnCustomContainer;

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
