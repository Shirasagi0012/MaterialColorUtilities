using Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColor
{
    private static readonly AttachedProperty<EventHandler?> SchemeChangedHandlerProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, EventHandler?>("SchemeChangedHandler");
    
    static MaterialColor()
    {
        SchemeProperty.Changed.AddClassHandler<AvaloniaObject>((o, args) =>
        {
            if (args.OldValue is ColorScheme prevScheme)
            {
                var oldHandler = o.GetValue(SchemeChangedHandlerProperty);
                if (oldHandler != null)
                {
                    prevScheme.SchemeChanged -= oldHandler;
                }
            }

            if (args.NewValue is not ColorScheme nextScheme)
            {
                ClearResolvers(o);
                o.ClearValue(SchemeChangedHandlerProperty);
                return;
            }

            WeakReference<AvaloniaObject> weakObj = new(o);
            EventHandler? newHandler = null;

            newHandler = (sender, e) =>
            {
                if (weakObj.TryGetTarget(out var target))
                {
                    if (sender is ColorScheme scheme)
                    {
                        SetResolvers(target, new MaterialColorScheme(scheme));
                    }
                }
                else
                {
                    if (sender is ColorScheme scheme && newHandler != null)
                    {
                        scheme.SchemeChanged -= newHandler;
                    }
                }
            };

            nextScheme.SchemeChanged += newHandler;
            o.SetValue(SchemeChangedHandlerProperty, newHandler);

            SetResolvers(o, new MaterialColorScheme(nextScheme));
        });
    }

    public static readonly AttachedProperty<ColorScheme?> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, ColorScheme?>("Scheme");

    public static ColorScheme? GetScheme(AvaloniaObject element)
    {
        return element.GetValue(SchemeProperty);
    }

    public static void SetScheme(AvaloniaObject element, ColorScheme? value)
    {
        element.SetValue(SchemeProperty, value);
    }

    private static void ClearResolvers(AvaloniaObject element)
    {
        element.ClearValue(MaterialColorSchemeHost.SysColorHostProperty);
        element.ClearValue(MaterialColorSchemeHost.RefPaletteHostProperty);
    }

    private static void SetResolvers(AvaloniaObject element, MaterialColorScheme resolver)
    {
        element.SetValue(MaterialColorSchemeHost.SysColorHostProperty, resolver);
        element.SetValue(MaterialColorSchemeHost.RefPaletteHostProperty, resolver);
    }
}
