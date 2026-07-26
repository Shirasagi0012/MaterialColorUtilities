using Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia;

/// <summary>
/// Carries a color scheme into a control tree.
/// </summary>
/// <remarks>
/// Two properties, one authoring and one distributing. App authors set <see cref="SchemeProperty" />
/// on whatever element should own the scheme; a class handler snapshots it into the inherited
/// <see cref="ResolvedSchemeProperty" />, which is what bindings and controls actually read. The
/// split exists because <see cref="ColorScheme" /> is mutable and bindable — its seed color can be
/// a <c>DynamicResource</c>, and custom colors can be added at runtime — while everything
/// downstream must see an immutable value.
/// </remarks>
public class MaterialColor : AvaloniaObject
{
    /// <summary>
    /// The scheme owned by this element. Not inherited: it names the element that declares the
    /// scheme, not the elements affected by it.
    /// </summary>
    public static readonly AttachedProperty<ColorScheme?> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, ColorScheme?>("Scheme");

    /// <summary>
    /// The snapshot every consumer reads, inherited down the tree. Set by this class from
    /// <see cref="SchemeProperty" />, or by a higher-level theming layer that owns its own scheme
    /// object; app authors should not assign it directly.
    /// </summary>
    public static readonly AttachedProperty<ResolvedColorScheme?> ResolvedSchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, ResolvedColorScheme?>(
            "ResolvedScheme",
            inherits: true);

    private static readonly AttachedProperty<EventHandler?> SchemeChangedHandlerProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, EventHandler?>("SchemeChangedHandler");

    static MaterialColor()
    {
        SchemeProperty.Changed.AddClassHandler<AvaloniaObject>(static (element, args) =>
        {
            if (args.OldValue is ColorScheme previous && element.GetValue(SchemeChangedHandlerProperty) is { } stale)
            {
                previous.SchemeChanged -= stale;
                element.ClearValue(SchemeChangedHandlerProperty);
            }

            if (args.NewValue is not ColorScheme next)
            {
                element.ClearValue(ResolvedSchemeProperty);
                return;
            }

            // The scheme outlives the element it is attached to (an app-level scheme is rooted for
            // the process lifetime), so the subscription must not keep the element alive.
            WeakReference<AvaloniaObject> weakElement = new(element);
            EventHandler? handler = null;
            handler = (sender, _) =>
            {
                if (sender is not ColorScheme scheme)
                    return;

                if (weakElement.TryGetTarget(out var target))
                    target.SetValue(ResolvedSchemeProperty, new ResolvedColorScheme(scheme));
                else if (handler is { })
                    scheme.SchemeChanged -= handler;
            };

            next.SchemeChanged += handler;
            element.SetValue(SchemeChangedHandlerProperty, handler);
            element.SetValue(ResolvedSchemeProperty, new ResolvedColorScheme(next));
        });
    }

    public static ColorScheme? GetScheme(AvaloniaObject element)
    {
        return element.GetValue(SchemeProperty);
    }

    public static void SetScheme(AvaloniaObject element, ColorScheme? value)
    {
        element.SetValue(SchemeProperty, value);
    }

    public static ResolvedColorScheme? GetResolvedScheme(AvaloniaObject element)
    {
        return element.GetValue(ResolvedSchemeProperty);
    }

    public static void SetResolvedScheme(AvaloniaObject element, ResolvedColorScheme? value)
    {
        element.SetValue(ResolvedSchemeProperty, value);
    }
}
