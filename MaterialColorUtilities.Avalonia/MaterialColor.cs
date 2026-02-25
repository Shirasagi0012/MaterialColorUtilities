using Avalonia;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColor : AvaloniaObject
{
    static MaterialColor()
    {
        SchemeProperty.Changed.AddClassHandler<AvaloniaObject>((element, args) =>
        {
            var value = args.NewValue as ISchemeProvider;
            if (element.IsSet(SchemeHostProperty)) return;
            if (value is null)
                element.ClearValue(SchemeHostProperty);
            else
                element.SetValue(SchemeHostProperty, new MaterialColorScheme(element[!SchemeProperty]));
        });
    }
    
    internal static readonly AttachedProperty<MaterialColorScheme> SchemeHostProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, MaterialColorScheme>(
            "SchemeHost", inherits: true);

    internal static MaterialColorScheme GetSchemeHost(AvaloniaObject element)
    {
        return element.GetValue(SchemeHostProperty);
    }

    internal static void SetSchemeHost(AvaloniaObject element, MaterialColorScheme value)
    {
        element.SetValue(SchemeHostProperty, value);
    }
    
    public static readonly AttachedProperty<ISchemeProvider?> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, ISchemeProvider?>(
            "Scheme");
    
    public static ISchemeProvider? GetScheme(AvaloniaObject element)
    {
        return element.GetValue(SchemeProperty);
    }
    
    public static void SetScheme(AvaloniaObject element, ISchemeProvider? value)
    {
        element.SetValue(SchemeProperty, value);
    }
}
