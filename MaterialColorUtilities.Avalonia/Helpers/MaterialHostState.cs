using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal sealed class MaterialHostState : IDisposable
{
    private readonly MaterialColorBindingContext _context;
    private readonly Application? _application;
    private readonly PropertyChangedEventHandler _schemeChangedHandler;
    private readonly bool _observeThemeHost;
    private IDisposable? _hostSchemeSubscription;
    private IDisposable? _applicationSchemeSubscription;
    private MaterialColorScheme? _hostSchemeHost;
    private MaterialColorScheme? _applicationSchemeHost;
    private bool _isDisposed;

    public MaterialHostState(MaterialColorBindingContext context, Application? application)
    {
        _context = context;
        _application = application;
        _schemeChangedHandler = (_, _) => RaiseChanged();
        _observeThemeHost = _context.ExplicitThemeVariant is null && _context.DictionaryThemeVariant is null;

        if (_context.ProviderAnchor is { })
            _context.ProviderAnchor.OwnerChanged += OnProviderOwnerChanged;

        RebindHost();
    }

    public event EventHandler? Changed;

    public AvaloniaObject? HostObject { get; private set; }

    public IResourceProvider? Provider => _context.ProviderAnchor;

    public IThemeVariantHost? ThemeHost { get; private set; }

    public MaterialColorScheme.MaterialColorSchemeInternal? Scheme => _hostSchemeHost?.Internal ?? _applicationSchemeHost?.Internal;

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
        DisposeHostSchemeSubscription();
        DisposeApplicationSchemeSubscription();
        SetSchemeHost(ref _hostSchemeHost, null);
        SetSchemeHost(ref _applicationSchemeHost, null);
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
            RebindHostSchemeSubscription();
            RebindApplicationSchemeSubscription();
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

    private void RebindHostSchemeSubscription()
    {
        DisposeHostSchemeSubscription();
        SetSchemeHost(ref _hostSchemeHost, null);

        if (HostObject is null)
            return;

        _hostSchemeSubscription = HostObject
            .GetObservable(MaterialColor.SchemeHostProperty)
            .Subscribe(new Observer<MaterialColorScheme?>(OnHostSchemeChanged));
    }

    private void RebindApplicationSchemeSubscription()
    {
        DisposeApplicationSchemeSubscription();
        SetSchemeHost(ref _applicationSchemeHost, null);

        if (_application is null || ReferenceEquals(HostObject, _application))
            return;

        _applicationSchemeSubscription = _application
            .GetObservable(MaterialColor.SchemeHostProperty)
            .Subscribe(new Observer<MaterialColorScheme?>(OnApplicationSchemeChanged));
    }

    private void OnHostSchemeChanged(MaterialColorScheme? schemeHost)
    {
        SetSchemeHost(ref _hostSchemeHost, schemeHost);
        RaiseChanged();
    }

    private void OnApplicationSchemeChanged(MaterialColorScheme? schemeHost)
    {
        SetSchemeHost(ref _applicationSchemeHost, schemeHost);
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

    private void SetSchemeHost(ref MaterialColorScheme? current, MaterialColorScheme? next)
    {
        if (ReferenceEquals(current, next))
            return;

        if (current is { })
            current.Internal.PropertyChanged -= _schemeChangedHandler;

        current = next;

        if (current is { })
            current.Internal.PropertyChanged += _schemeChangedHandler;
    }

    private void DisposeHostSchemeSubscription()
    {
        _hostSchemeSubscription?.Dispose();
        _hostSchemeSubscription = null;
    }

    private void DisposeApplicationSchemeSubscription()
    {
        _applicationSchemeSubscription?.Dispose();
        _applicationSchemeSubscription = null;
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