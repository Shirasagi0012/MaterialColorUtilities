using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Gallery.Controls;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Gallery.ViewModels;

public partial class CustomColorEntryViewModel : ViewModelBase
{
    private readonly SchemePlaygroundViewModel _owner;
    private bool _syncingColor;

    public CustomColorEntryViewModel(SchemePlaygroundViewModel owner, string name, Color color)
    {
        _owner = owner;
        Resource = new CustomColor(name, color);
        _syncingColor = true;
        Name = name;
        SourceHex = ToHex(color);
        SelectedHct = HctSelection.FromHct(Hct.FromAvaloniaColor(color));
        _syncingColor = false;
    }

    public CustomColor Resource { get; }
    public string ResourceName => Resource.Name!;
    public ColorScheme? Scheme => _owner.Scheme;
    public Color SourceColor => Resource.Color!.Value;
    public IBrush SourceBrush => new SolidColorBrush(SourceColor);
    public string HarmonizeState => Harmonize ? "On" : "Off";
    public Color HarmonizedColor => Scheme?.Color is { } seed
        ? global::MaterialColorUtilities.Blend.Blend.Harmonize(
            ArgbColor.FromAvaloniaColor(SourceColor), ArgbColor.FromAvaloniaColor(seed)).ToAvaloniaColor()
        : SourceColor;
    public IBrush HarmonizedBrush => new SolidColorBrush(HarmonizedColor);
    public string HarmonizedHex => ToHex(HarmonizedColor);

    [ObservableProperty] public partial string Name { get; set; } = string.Empty;
    [ObservableProperty] public partial string? NameError { get; set; }
    [ObservableProperty] public partial string SourceHex { get; set; } = string.Empty;
    [ObservableProperty] public partial string? SourceError { get; set; }
    public string ExpansionAction => IsExpanded ? "Collapse custom color" : "Expand custom color";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpansionAction))]
    public partial bool IsExpanded { get; set; } = true;
    [ObservableProperty] public partial HctSelection SelectedHct { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HarmonizeState))]
    public partial bool Harmonize { get; set; } = true;

    partial void OnNameChanged(string value) => _owner.ValidateCustomColorNames();

    internal void AcceptName(string name)
    {
        NameError = null;
        if (Resource.Name == name) return;
        Resource.Name = name;
        OnPropertyChanged(nameof(ResourceName));
    }

    partial void OnSourceHexChanged(string value)
    {
        if (_syncingColor) return;
        var hex = value?.Trim() ?? string.Empty;
        if (hex.StartsWith('#')) hex = hex[1..];
        if (hex.Length != 6 || !uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out var rgb))
        {
            SourceError = "Enter a six-digit HEX color.";
            return;
        }
        SetColor(Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb), fromHct: false);
    }

    partial void OnSelectedHctChanged(HctSelection value)
    {
        if (!_syncingColor) SetColor(value.ToHct().ToAvaloniaColor(), fromHct: true);
    }

    private void SetColor(Color color, bool fromHct)
    {
        _syncingColor = true;
        try
        {
            Resource.Color = color;
            SourceError = null;
            if (fromHct) SourceHex = ToHex(color);
            else SelectedHct = HctSelection.FromHct(Hct.FromAvaloniaColor(color));
        }
        finally { _syncingColor = false; }
        OnPropertyChanged(nameof(SourceColor));
        OnPropertyChanged(nameof(SourceBrush));
        NotifyHarmonizedColor();
    }

    partial void OnHarmonizeChanged(bool value) => Resource.Harmonize = value;

    internal void NotifySchemeChanged()
    {
        OnPropertyChanged(nameof(Scheme));
        NotifyHarmonizedColor();
    }

    private void NotifyHarmonizedColor()
    {
        OnPropertyChanged(nameof(HarmonizedColor));
        OnPropertyChanged(nameof(HarmonizedBrush));
        OnPropertyChanged(nameof(HarmonizedHex));
    }

    [RelayCommand] private void Remove() => _owner.RemoveCustomColor(this);
    [RelayCommand] private void ToggleExpanded() => IsExpanded = !IsExpanded;

    private static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
