using System.Reflection;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal static class MaterialMarkupExtensionHelper
{
    public static bool ShouldProvideBrush(IProvideValueTarget provideValueTarget)
    {
        if (provideValueTarget.TargetProperty switch
            {
                AvaloniaProperty avaloniaProperty => avaloniaProperty.PropertyType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => null
            } is not { } type)
            return true;

        return type != typeof(Color) && type != typeof(Color?);
    }
}
