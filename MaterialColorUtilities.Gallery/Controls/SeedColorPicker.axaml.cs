using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace MaterialColorUtilities.Gallery.Controls;

public partial class SeedColorPicker : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SeedColorPicker, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<HctSelection> HctProperty =
        AvaloniaProperty.Register<SeedColorPicker, HctSelection>(
            nameof(Hct),
            defaultValue: default,
            defaultBindingMode: BindingMode.TwoWay);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public HctSelection Hct
    {
        get => GetValue(HctProperty);
        set => SetValue(HctProperty, value);
    }

    public SeedColorPicker()
    {
        InitializeComponent();
    }
}
