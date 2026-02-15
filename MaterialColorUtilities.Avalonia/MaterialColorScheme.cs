using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.DynamicColors;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColorScheme : AvaloniaObject
{
    public static readonly DirectProperty<MaterialColorScheme, ISchemeProvider?> SchemeProperty =
        AvaloniaProperty.RegisterDirect<MaterialColorScheme, ISchemeProvider?>(
            nameof(Scheme),
            palette => palette.Scheme,
            (palette, provider) => palette.Scheme = provider
        );

    public ISchemeProvider? Scheme
    {
        get;
        set => SetAndRaise(SchemeProperty, ref field, value);
    }

    private DynamicScheme? _scheme;

    public MaterialColorScheme()
    {
        SchemeProperty.Changed.AddClassHandler<MaterialColorScheme, ISchemeProvider?>((target, args) =>
        {
            target._scheme = args.NewValue.GetValueOrDefault()?.CreateScheme(ThemeVariant.Default);
        });
    }

    public Color? GetColor(string color)
        => color switch
        {
            "primary" => _scheme?.Primary.ToAvaloniaColor(),
            "onPrimary" => _scheme?.OnPrimary.ToAvaloniaColor(),
            "primaryContainer" => _scheme?.PrimaryContainer.ToAvaloniaColor(),
            "onPrimaryContainer" => _scheme?.OnPrimaryContainer.ToAvaloniaColor(),
            _ => Colors.Black
        };
    
    public MaterialColorScheme ProvideValue(IServiceProvider serviceProvider) => this;
}
