using MaterialColorUtilities.Avalonia;

namespace MaterialColorUtilities.Gallery.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";
    public MaterialColorScheme Scheme { get; }

    public MainWindowViewModel()
    {
        Scheme = new MaterialColorScheme();
    }
}
