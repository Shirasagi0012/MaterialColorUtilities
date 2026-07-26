using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Tokens;

// Vendored from the DesignTokens.Avalonia package — see TokenPrimitives.cs.

/// <summary>
/// Tracks which resolver and theme variant apply to a binding, following the host object as the
/// resource provider's owner and the ambient theme variant change.
/// </summary>
internal sealed class TokenHostState<TValue, TKey, TTokenHost> : IDisposable
    where TTokenHost : ITokenHost<TValue, TKey, TTokenHost>
{
    private readonly TokenBindingContext _context;
    private readonly Application? _application;
    private readonly bool _observeThemeHost;
    private IDisposable? _hostResolverSubscription;
    private IDisposable? _applicationResolverSubscription;
    private ITokenResolver<TValue, TKey>? _hostResolver;
    private ITokenResolver<TValue, TKey>? _applicationResolver;
    private bool _isDisposed;

    public TokenHostState(TokenBindingContext context, Application? application)
    {
        _context = context;
        _application = application;
        _observeThemeHost = _context.ExplicitThemeVariant is null && _context.DictionaryThemeVariant is null;

        if (_context.ProviderAnchor is { })
            _context.ProviderAnchor.OwnerChanged += OnProviderOwnerChanged;

        RebindHost();
    }

    public event EventHandler? Changed;

    public AvaloniaObject? HostObject { get; private set; }

    public IResourceProvider? Provider => _context.ProviderAnchor;

    public IThemeVariantHost? ThemeHost { get; private set; }

    public ITokenResolver<TValue, TKey>? Resolver => _hostResolver ?? _applicationResolver;

    public ThemeVariant ThemeVariant =>
        _context.ExplicitThemeVariant
        ?? _context.DictionaryThemeVariant
        ?? ThemeHost?.ActualThemeVariant
        ?? (_application as IThemeVariantHost)?.ActualThemeVariant
        ?? ThemeVariant.Light;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_context.ProviderAnchor is { })
            _context.ProviderAnchor.OwnerChanged -= OnProviderOwnerChanged;

        DetachThemeHost(ThemeHost);
        DisposeHostResolverSubscription();
        DisposeApplicationResolverSubscription();
        _hostResolver = null;
        _applicationResolver = null;
    }

    private void OnProviderOwnerChanged(object? sender, EventArgs e)
    {
        RebindHost();
        RaiseChanged();
    }

    private void RebindHost()
    {
        var nextHostObject = ResolveHostObject();
        var nextThemeHost = ResolveThemeHost(nextHostObject);

        if (!ReferenceEquals(HostObject, nextHostObject))
        {
            HostObject = nextHostObject;
            RebindHostResolverSubscription();
            RebindApplicationResolverSubscription();
        }

        if (!ReferenceEquals(ThemeHost, nextThemeHost))
        {
            DetachThemeHost(ThemeHost);
            ThemeHost = nextThemeHost;
            AttachThemeHost(ThemeHost);
        }
    }

    private AvaloniaObject? ResolveHostObject()
    {
        if (IsTargetHost(_context.TargetObject))
            return _context.TargetObject;

        if (_context.ProviderAnchor is { })
            return _context.ProviderAnchor.Owner as AvaloniaObject ?? _application;

        if (_context.Anchor is AvaloniaObject anchorObject)
            return anchorObject;

        return _application;
    }

    private IThemeVariantHost? ResolveThemeHost(AvaloniaObject? hostObject)
    {
        return hostObject as IThemeVariantHost
               ?? _context.ProviderAnchor?.Owner as IThemeVariantHost
               ?? _context.Anchor as IThemeVariantHost
               ?? _application;
    }

    private void RebindHostResolverSubscription()
    {
        DisposeHostResolverSubscription();
        _hostResolver = null;

        if (HostObject is null)
            return;

        _hostResolverSubscription = TTokenHost
            .GetTokenObservable(HostObject)
            .Subscribe(new Observer<ITokenResolver<TValue, TKey>?>(OnHostResolverChanged));
    }

    private void RebindApplicationResolverSubscription()
    {
        DisposeApplicationResolverSubscription();
        _applicationResolver = null;

        if (_application is null || ReferenceEquals(HostObject, _application))
            return;

        _applicationResolverSubscription = TTokenHost
            .GetTokenObservable(_application)
            .Subscribe(new Observer<ITokenResolver<TValue, TKey>?>(OnApplicationResolverChanged));
    }

    private void OnHostResolverChanged(ITokenResolver<TValue, TKey>? resolver)
    {
        _hostResolver = resolver;
        RaiseChanged();
    }

    private void OnApplicationResolverChanged(ITokenResolver<TValue, TKey>? resolver)
    {
        _applicationResolver = resolver;
        RaiseChanged();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        RaiseChanged();
    }

    private void AttachThemeHost(IThemeVariantHost? themeHost)
    {
        if (_observeThemeHost && themeHost is { })
            themeHost.ActualThemeVariantChanged += OnActualThemeVariantChanged;
    }

    private void DetachThemeHost(IThemeVariantHost? themeHost)
    {
        if (_observeThemeHost && themeHost is { })
            themeHost.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
    }

    private void DisposeHostResolverSubscription()
    {
        _hostResolverSubscription?.Dispose();
        _hostResolverSubscription = null;
    }

    private void DisposeApplicationResolverSubscription()
    {
        _applicationResolverSubscription?.Dispose();
        _applicationResolverSubscription = null;
    }

    private void RaiseChanged()
    {
        if (!_isDisposed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsTargetHost(AvaloniaObject? targetObject)
    {
        return targetObject is StyledElement or Application or IResourceHost or IThemeVariantHost;
    }
}

internal sealed class TokenObservable<TValue, TKey, TTokenHost>(
    TokenBindingContext context,
    Application? application,
    TValue? fallbackValue,
    Func<ITokenResolver<TValue, TKey>?, ThemeVariant, AvaloniaObject?, TValue?, TValue?> resolveValue
) : IObservable<TValue?>
    where TTokenHost : ITokenHost<TValue, TKey, TTokenHost>
{
    private readonly TokenBindingContext _context = context;
    private readonly Application? _application = application;
    private readonly TValue? _fallbackValue = fallbackValue;

    private readonly Func<ITokenResolver<TValue, TKey>?, ThemeVariant, AvaloniaObject?, TValue?, TValue?>
        _resolveValue = resolveValue;

    public IDisposable Subscribe(IObserver<TValue?> observer)
    {
        return new Subscription(this, observer);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly TokenObservable<TValue, TKey, TTokenHost> _owner;
        private readonly IObserver<TValue?> _observer;
        private readonly TokenHostState<TValue, TKey, TTokenHost> _hostState;
        private bool _isDisposed;

        public Subscription(TokenObservable<TValue, TKey, TTokenHost> owner, IObserver<TValue?> observer)
        {
            _owner = owner;
            _observer = observer;
            _hostState = new TokenHostState<TValue, TKey, TTokenHost>(_owner._context, _owner._application);
            _hostState.Changed += OnHostStateChanged;

            Publish();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _hostState.Changed -= OnHostStateChanged;
            _hostState.Dispose();
        }

        private void OnHostStateChanged(object? sender, EventArgs e)
        {
            Publish();
        }

        private void Publish()
        {
            if (_isDisposed)
                return;

            var value = _owner._resolveValue(
                _hostState.Resolver,
                _hostState.ThemeVariant,
                _hostState.HostObject,
                _owner._fallbackValue
            );

            _observer.OnNext(value);
        }
    }
}
