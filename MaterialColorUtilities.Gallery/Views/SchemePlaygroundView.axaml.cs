using Avalonia.Controls;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Gallery.Views;

public partial class SchemePlaygroundView : UserControl
{
    // Avalonia XAML cannot call struct constructors with x:Arguments. Expose
    // ordinary application key values for native x:Static/DynamicResource use.
    public static RefPaletteKey Primary60Key { get; } = new(RefPaletteToken.Primary, 60);
    public static RefPaletteKey Brand60Key { get; } = new("Brand", 60);
    public static CustomColorKey BrandContainerKey { get; } = new("Brand", CustomColorRole.Container);
    public static CustomColorKey BrandOnContainerKey { get; } = new("Brand", CustomColorRole.OnContainer);

    public SchemePlaygroundView()
    {
        InitializeComponent();
    }
}
