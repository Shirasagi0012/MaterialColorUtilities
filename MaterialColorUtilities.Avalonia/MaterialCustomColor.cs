using Avalonia;
using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia;

public sealed class MaterialCustomColor : AvaloniaObject
{
    public static readonly StyledProperty<string> KeyProperty =
        AvaloniaProperty.Register<MaterialCustomColor, string>(nameof(Key), String.Empty);

    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<MaterialCustomColor, Color?>(nameof(Color));

    public static readonly StyledProperty<bool> BlendProperty =
        AvaloniaProperty.Register<MaterialCustomColor, bool>(nameof(Blend), true);

    public MaterialCustomColor()
    {
    }

    public MaterialCustomColor(string key, Color color, bool blend = true)
    {
        Key = key;
        Color = color;
        Blend = blend;
    }

    public string Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public bool Blend
    {
        get => GetValue(BlendProperty);
        set => SetValue(BlendProperty, value);
    }
}