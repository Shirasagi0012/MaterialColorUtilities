using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Tokens;

// Vendored from the DesignTokens.Avalonia package — see TokenPrimitives.cs.

internal sealed class TokenBindingContext(
    object? anchor,
    IResourceProvider? providerAnchor,
    AvaloniaObject? targetObject,
    ThemeVariant? explicitThemeVariant,
    ThemeVariant? dictionaryThemeVariant
)
{
    public object? Anchor { get; } = anchor;

    public IResourceProvider? ProviderAnchor { get; } = providerAnchor;

    public AvaloniaObject? TargetObject { get; } = targetObject;

    public ThemeVariant? ExplicitThemeVariant { get; } = explicitThemeVariant;

    public ThemeVariant? DictionaryThemeVariant { get; } = dictionaryThemeVariant;
}

internal static class TokenBinding
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

    public static IObservable<TValue?> CreateObservable<TValue, TKey, TTokenHost>(
        TokenBindingContext context,
        TokenKey<TValue, TKey> key,
        TValue? fallbackValue = default!
    ) where TTokenHost : ITokenHost<TValue, TKey, TTokenHost>
    {
        return new TokenObservable<TValue, TKey, TTokenHost>(context, Application.Current, fallbackValue, ResolveValue);

        TValue? ResolveValue(
            ITokenResolver<TValue, TKey>? resolver,
            ThemeVariant themeVariant,
            AvaloniaObject? hostObject,
            TValue? fallback
        )
        {
            return resolver is { } && resolver.TryResolve(key, themeVariant, hostObject, out var value) && value is { }
                ? value
                : fallback;
        }
    }

    public static TokenBindingContext CaptureContext(
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

        return new TokenBindingContext(
            anchor,
            providerAnchor,
            targetObject,
            explicitThemeVariant,
            dictionaryVariant);
    }

    private static T? FindFirstParent<T>(IAvaloniaXamlIlParentStackProvider parentStack) where T : class
    {
        foreach (var parent in parentStack.Parents)
            if (parent is T typed)
                return typed;

        return null;
    }
}

internal static class TokenExtensionHelper<TValue, TKey, TTokenHost>
    where TTokenHost : ITokenHost<TValue, TKey, TTokenHost>
{
    public static IObservable<TValue?> ProvideObservable(
        IServiceProvider serviceProvider,
        TokenKey<TValue, TKey> tokenKey,
        ThemeVariant? theme,
        TValue? fallbackValue
    )
    {
        var (target, parentStack) = TokenBinding.GetContextServices(serviceProvider);

        var targetObject = target.TargetObject as AvaloniaObject;
        var context = TokenBinding.CaptureContext(parentStack, targetObject, theme);
        return TokenBinding.CreateObservable<TValue, TKey, TTokenHost>(context, tokenKey, fallbackValue);
    }
}
