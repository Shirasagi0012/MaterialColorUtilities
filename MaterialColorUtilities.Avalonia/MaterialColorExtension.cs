using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColorExtension
{
    public MaterialColorExtension()
    {
    }

    public MaterialColorExtension(string color)
    {
        Color = color;
    }

    [ConstructorArgument("color")] public string Color { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (
            serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget
            {
                TargetObject: AvaloniaObject obj
            } provideValueTarget
            && MaterialColor.GetScheme(obj)?.GetColor(Color) is { } color
        )
        {
            return color;
        }

        if (serviceProvider.GetService(typeof(IAvaloniaXamlIlParentStackProvider)) is
            IAvaloniaXamlIlEagerParentStackProvider { Parents: { } parents })
        {
            foreach (var parent in parents)
            {
                if (parent is AvaloniaObject obj1 && MaterialColor.GetScheme(obj1)?.GetColor(Color) is { } color1)
                {
                    return color1;
                }
            }
        }
        
        return Colors.Transparent;
    }
}
