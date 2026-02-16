using Avalonia;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColor : AvaloniaObject
{
    public static readonly StyledProperty<MaterialColorScheme?> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, MaterialColorScheme?>(
            "Scheme", inherits: true);

    public static MaterialColorScheme? GetScheme(AvaloniaObject? element)
    {
        return element?.GetValue(SchemeProperty);
    }

    public static void SetScheme(AvaloniaObject element, MaterialColorScheme? value)
    {
        element.SetValue(SchemeProperty, value);
    }
}
