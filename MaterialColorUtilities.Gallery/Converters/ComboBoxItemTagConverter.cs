using System;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Gallery.ViewModels;

namespace MaterialColorUtilities.Gallery.Converters;

public class ComboBoxItemTagConverterExtension : MarkupExtension
{
    private static FuncValueConverter<Type, string> _funcValueConverter = new(t =>
        t is { } && SchemePlaygroundViewModel.SchemeNames.TryGetValue(t, out var name)
            ? name
            : "<unknown>");

    public override object ProvideValue(IServiceProvider serviceProvider) => _funcValueConverter;
}
