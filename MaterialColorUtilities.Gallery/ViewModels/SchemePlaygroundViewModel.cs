using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Gallery.Controls;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Gallery.ViewModels;

public partial class SchemePlaygroundViewModel :ViewModelBase
{
    private static readonly Color DefaultSeedColor = Color.FromRgb(0x67, 0x50, 0xa4);

    public static IReadOnlyList<Type> SchemeTypes { get; } =
    [
        typeof(TonalSpotScheme),
        typeof(ExpressiveScheme),
        typeof(FidelityScheme),
        typeof(ContentScheme),
        typeof(FruitSaladScheme),
        typeof(MonochromeScheme),
        typeof(NeutralScheme),
        typeof(RainbowScheme),
        typeof(VibrantScheme),
    ];

    public static IReadOnlyDictionary<Type, string> SchemeNames { get; } = new Dictionary<Type, string>
    {
        [typeof(TonalSpotScheme)] = "Tonal Spot",
        [typeof(ExpressiveScheme)] = "Expressive",
        [typeof(FidelityScheme)] = "Fidelity",
        [typeof(ContentScheme)] = "Content",
        [typeof(FruitSaladScheme)] = "Fruit Salad",
        [typeof(MonochromeScheme)] = "Monochrome",
        [typeof(NeutralScheme)] = "Neutral",
        [typeof(RainbowScheme)] = "Rainbow",
        [typeof(VibrantScheme)] = "Vibrant",
    };

    [ObservableProperty] public partial ISchemeProvider? Scheme { get; set; } = new TonalSpotScheme(DefaultSeedColor);
    [ObservableProperty] public partial HctSelection SelectedHct { get; set; } = HctSelection.FromHct(Hct.FromAvaloniaColor(DefaultSeedColor));
    [ObservableProperty] public partial Type SchemeType { get; set; } = typeof(TonalSpotScheme);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(SchemeType):
            case nameof(SelectedHct):
                Scheme = CreateScheme(SchemeType, SelectedHct);
                break;
        }
    }

    private static ISchemeProvider CreateScheme(Type type, HctSelection hctSelection)
    {
        var seedColor = hctSelection.ToHct().ToAvaloniaColor();

        if (type == typeof(TonalSpotScheme))
            return new TonalSpotScheme(seedColor);
        if (type == typeof(ExpressiveScheme))
            return new ExpressiveScheme(seedColor);
        if (type == typeof(FidelityScheme))
            return new FidelityScheme(seedColor);
        if (type == typeof(ContentScheme))
            return new ContentScheme(seedColor);
        if (type == typeof(FruitSaladScheme))
            return new FruitSaladScheme(seedColor);
        if (type == typeof(MonochromeScheme))
            return new MonochromeScheme(seedColor);
        if (type == typeof(NeutralScheme))
            return new NeutralScheme(seedColor);
        if (type == typeof(RainbowScheme))
            return new RainbowScheme(seedColor);
        if (type == typeof(VibrantScheme))
            return new VibrantScheme(seedColor);
        throw new NotSupportedException($"Scheme type {type} is not supported.");
    }
}
