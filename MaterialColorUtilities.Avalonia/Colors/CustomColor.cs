using Avalonia;
using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia;

using System.ComponentModel;
using System.Globalization;
using global::Avalonia.Data;

[TypeConverter(typeof(CustomColorConverter))]
public sealed class CustomColor : AvaloniaObject
{
    public CustomColor() {}

    public CustomColor(Color color)
    {
        Color = color;
    }

    public CustomColor(IBinding binding)
    {
        this[!ColorProperty] = binding;
    }

    public CustomColor(string colorString)
    {
        if (Color.TryParse(s: colorString, color: out var color))
            Color = color;

        throw new FormatException($"'{colorString}' is not a valid color string.");
    }

    public readonly static StyledProperty<Color> ColorProperty
        = AvaloniaProperty.Register<CustomColor, Color>(name: nameof(Color), defaultValue: Colors.Transparent);

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(property: ColorProperty, value: value);
    }

    public IBinding ColorBinding => this[!ColorProperty];

    public IBinding ProvideTypedValue(IServiceProvider serviceProvider) => this[!ColorProperty];
    public IBinding ProvideValue(ISchemeProvider serviceProvider) => this[!ColorProperty];
}

public sealed class CustomColorConverter : TypeConverter
{
    override public bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || sourceType == typeof(Color);

    override public object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value
    )
    {
        if (value is string s && Color.TryParse(s: s, color: out var color))
            return new CustomColor
            {
                Color = color,
            };

        if (value is Color c)
            return new CustomColor
            {
                Color = c,
            };

        throw new NotSupportedException($"Cannot convert {value} from {value?.GetType().FullName ?? "null"} to Color");
    }

}
