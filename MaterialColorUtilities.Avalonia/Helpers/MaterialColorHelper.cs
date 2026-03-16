using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal static class MaterialColorHelper
{
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

    public static IObservable<Color> ProvideSysColorBinding(
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        Color? fallback = null,
        ThemeVariant? themeVariant = null,
        AvaloniaObject? targetObject = null
    )
    {
        if (TokenHelper.IsCustom(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var fallbackColor = fallback ?? Colors.Transparent;
        var normalizedKey = customKey?.Trim();
        var context = CaptureContext(parentStack, targetObject, themeVariant);

        return CreateSysColorObservable(context, token, normalizedKey, fallbackColor);
    }

    public static IObservable<Color> ProvideRefColorBinding(
        IAvaloniaXamlIlParentStackProvider parentStack,
        RefPaletteToken token,
        byte tone,
        string? customKey = null,
        Color? fallback = null,
        AvaloniaObject? targetObject = null
    )
    {
        if (TokenHelper.IsCustom(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var fallbackColor = fallback ?? Colors.Transparent;
        var normalizedKey = customKey?.Trim();
        var context = CaptureContext(parentStack, targetObject);

        return CreateRefColorObservable(context, token, tone, normalizedKey, fallbackColor);
    }

    internal static IObservable<Color> CreateSysColorObservable(
        MaterialColorBindingContext context,
        SysColorToken token,
        string? customKey,
        Color fallbackColor
    )
    {
        return new MaterialColorObservable(context, Application.Current, fallbackColor, ResolveColor);

        Color ResolveColor(MaterialColorScheme.MaterialColorSchemeInternal? scheme, ThemeVariant theme) =>
            scheme?.ResolveSys(token, theme, customKey) ?? fallbackColor;
    }

    internal static IObservable<Color> CreateRefColorObservable(
        MaterialColorBindingContext context,
        RefPaletteToken token,
        byte tone,
        string? customKey,
        Color fallbackColor
    )
    {
        return new MaterialColorObservable(context, Application.Current, fallbackColor, ResolveColor);

        Color ResolveColor(MaterialColorScheme.MaterialColorSchemeInternal? scheme, ThemeVariant theme) =>
            scheme?.ResolveRef(token, tone, customKey) ?? fallbackColor;
    }

    public static MaterialColorBindingContext CaptureContext(
        IAvaloniaXamlIlParentStackProvider parentStack,
        AvaloniaObject? targetObject = null,
        ThemeVariant? explicitThemeVariant = null
    )
    {
        var providerAnchor = FindFirstParent<IResourceProvider>(parentStack);
        var anchor = (object?)providerAnchor
                     ?? FindFirstParent<IResourceHost>(parentStack)
                     ?? FindFirstParent<StyledElement>(parentStack);

        ThemeVariant? dictionaryVariant = null;
        foreach (var parent in parentStack.Parents)
            if (parent is IThemeVariantProvider { Key: { } setKey })
            {
                dictionaryVariant = setKey;
                break;
            }

        return new MaterialColorBindingContext(
            anchor,
            providerAnchor,
            targetObject,
            explicitThemeVariant,
            dictionaryVariant
        );
    }

    public static bool ShouldProvideBrush(IProvideValueTarget provideValueTarget)
    {
        if (provideValueTarget.TargetProperty switch
            {
                AvaloniaProperty avaloniaProperty => avaloniaProperty.PropertyType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => null
            } is not { } type)
            return true;

        return type != typeof(Color) && type != typeof(Color?);
    }

    private static T? FindFirstParent<T>(IAvaloniaXamlIlParentStackProvider parentStack) where T : class
    {
        foreach (var parent in parentStack.Parents)
            if (parent is T typed)
                return typed;

        return null;
    }
}