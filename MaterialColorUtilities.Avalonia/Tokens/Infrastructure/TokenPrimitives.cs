using Avalonia;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Tokens;

// Vendored from the DesignTokens.Avalonia package (MIT, github.com/shirasagi0012/DesignTokens),
// which is being retired. Kept internal so none of it reaches this package's public surface.
// The theming core v3 rework replaces this whole layer; see ROADMAP.md phase 1.

/// <summary>
/// Identifies a token that resolves to <typeparamref name="TValue" />.
/// </summary>
/// <typeparam name="TValue">The resolved token value type.</typeparam>
/// <typeparam name="TKey">The token key type.</typeparam>
internal sealed class TokenKey<TValue, TKey>
{
    public TokenKey()
    {
    }

    /// <param name="value">Opaque key data understood by the resolver.</param>
    public TokenKey(TKey? value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the opaque key payload carried by this token key.
    /// </summary>
    public TKey? Value { get; }
}

internal interface ITokenResolver<TValue, TKey>
{
    bool TryResolve(
        TokenKey<TValue, TKey> key,
        ThemeVariant themeVariant,
        AvaloniaObject? hostObject,
        out TValue value
    );
}

internal interface ITokenHost<TValue, TKey, TTokenHost>
    where TTokenHost : ITokenHost<TValue, TKey, TTokenHost>
{
    static abstract IObservable<ITokenResolver<TValue, TKey>?> GetTokenObservable(AvaloniaObject element);
    static abstract ITokenResolver<TValue, TKey>? GetResolver(AvaloniaObject element);
    static abstract void SetResolver(AvaloniaObject element, ITokenResolver<TValue, TKey>? value);
    static abstract void ClearResolver(AvaloniaObject element);
}

internal sealed class Observer<T>(Action<T> onNext) : IObserver<T>
{
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(T value)
    {
        onNext(value);
    }
}
