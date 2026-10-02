using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.Gallery.Controls;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Gallery.ViewModels;

public partial class SchemePlaygroundViewModel : ViewModelBase
{
    private static readonly Color DefaultSeedColor = Color.FromRgb(0x67, 0x50, 0xa4);
    private bool _syncing;

    private static readonly IReadOnlyList<SpecOption> AllSpecOptions =
    [
        new("(2021) Material Design 3", ColorSpec.SpecVersion.Spec2021),
        new("(2025) Material 3 Expressive", ColorSpec.SpecVersion.Spec2025),
        new("(2026) Material Design", ColorSpec.SpecVersion.Spec2026),
    ];

    private static readonly IReadOnlyList<SchemeOption> AllSchemeOptions =
    [
        new("Tonal Spot", typeof(TonalSpotScheme), Has2021: SchemeAvailability.Available, Has2025: SchemeAvailability.Available,
            Has2026: SchemeAvailability.FallbackOnly),
        new("Expressive", typeof(ExpressiveScheme), Has2021: SchemeAvailability.Available, Has2025: SchemeAvailability.Available,
            Has2026: SchemeAvailability.FallbackOnly),
        new("Fidelity", typeof(FidelityScheme)),
        new("Content", typeof(ContentScheme)),
        new("Fruit Salad", typeof(FruitSaladScheme)),
        new("Monochrome", typeof(MonochromeScheme)),
        new("Neutral", typeof(NeutralScheme), Has2021: SchemeAvailability.Available, Has2025: SchemeAvailability.Available,
            Has2026: SchemeAvailability.FallbackOnly),
        new("Rainbow", typeof(RainbowScheme)),
        new("Vibrant", typeof(VibrantScheme), Has2021: SchemeAvailability.Available, Has2025: SchemeAvailability.Available,
            Has2026: SchemeAvailability.FallbackOnly),
        new("CMF", typeof(CmfScheme), Has2021: SchemeAvailability.Unavailable, Has2025: SchemeAvailability.Unavailable,
            Has2026: SchemeAvailability.Available, SupportsSecondSeed: true)
    ];

    [ObservableProperty] public partial ColorScheme? Scheme { get; set; }

    [ObservableProperty]
    public partial HctSelection SelectedHct { get; set; } =
        HctSelection.FromHct(Hct.FromAvaloniaColor(DefaultSeedColor));

    [ObservableProperty]
    public partial HctSelection SelectedSecondaryHct { get; set; } =
        HctSelection.FromHct(Hct.FromAvaloniaColor(DefaultSeedColor));

    public static IReadOnlyList<PlatformOption> PlatformOptions { get; } =
        [new("Phone", DynamicScheme.Platform.Phone), new("Watch", DynamicScheme.Platform.Watch)];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewThemeVariant))]
    public partial PlatformOption SelectedPlatformOption { get; set; } = PlatformOptions[0];

    // Watch color roles target dark surfaces; Phone inherits the surrounding theme.
    public ThemeVariant PreviewThemeVariant => SelectedPlatformOption.Platform == DynamicScheme.Platform.Watch
        ? ThemeVariant.Dark
        : ThemeVariant.Default;

    [ObservableProperty] public partial double SelectedContrast { get; set; }
    [ObservableProperty] public partial IReadOnlyList<SpecOption> SpecOptions { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedSpecVersion))]
    [NotifyPropertyChangedFor(nameof(IsFallback))]
    [NotifyPropertyChangedFor(nameof(IsSecondarySeedVisible))]
    public partial SpecOption SelectedSpecOption { get; set; } = AllSpecOptions[0];

    [ObservableProperty] public partial IReadOnlyList<SchemeOption> SchemeOptions { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFallback))]
    [NotifyPropertyChangedFor(nameof(IsSecondarySeedVisible))]
    public partial SchemeOption? SelectedSchemeOption { get; set; }

    public ColorSpec.SpecVersion SelectedSpecVersion => SelectedSpecOption?.Version ?? default;

    public bool IsSecondarySeedVisible =>
        SelectedSpecVersion == ColorSpec.SpecVersion.Spec2026 &&
        SelectedSchemeOption is { SupportsSecondSeed: true };

    public ObservableCollection<CustomColorEntryViewModel> CustomColors { get; } = [];
    public bool HasNoCustomColors => CustomColors.Count == 0;

    [RelayCommand]
    private void AddCustomColor()
    {
        var name = "Custom";
        for (var number = 2; CustomColors.Any(entry =>
                 string.Equals(entry.ResourceName, name, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(entry.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase)); number++)
            name = $"Custom {number}";
        foreach (var entry in CustomColors) entry.IsExpanded = false;
        AddCustomColor(name, Color.FromRgb(0xff, 0x57, 0x22));
    }

    private void AddCustomColor(string name, Color color)
    {
        var entry = new CustomColorEntryViewModel(this, name, color);
        CustomColors.Add(entry);
        Scheme?.CustomColors.Add(entry.Resource);
        OnPropertyChanged(nameof(HasNoCustomColors));
    }

    internal void RemoveCustomColor(CustomColorEntryViewModel entry)
    {
        var index = CustomColors.IndexOf(entry);
        if (index < 0) return;
        Scheme?.CustomColors.Remove(entry.Resource);
        CustomColors.Remove(entry);
        if (entry.IsExpanded && CustomColors.Count > 0)
            CustomColors[Math.Min(index, CustomColors.Count - 1)].IsExpanded = true;
        ValidateCustomColorNames();
        OnPropertyChanged(nameof(HasNoCustomColors));
    }

    internal void ValidateCustomColorNames()
    {
        // Revisit a draft when another accepted rename frees its requested name.
        bool changed;
        do
        {
            changed = false;
            foreach (var entry in CustomColors)
            {
                var name = entry.Name?.Trim() ?? string.Empty;
                if (name.Length == 0) entry.NameError = "Enter a name.";
                else if (CustomColors.Any(other => other != entry &&
                             string.Equals(other.ResourceName, name, StringComparison.OrdinalIgnoreCase)))
                    entry.NameError = "Use a unique name.";
                else
                {
                    changed |= entry.ResourceName != name;
                    entry.AcceptName(name);
                }
            }
        } while (changed);
    }

    public SchemePlaygroundViewModel()
    {
        RefreshSchemeOptions();
        RefreshSpecOptions();
        EnsureSecondarySeedDefault();
        RebuildScheme(true);
        AddCustomColor("Brand", Color.FromRgb(0xff, 0x57, 0x22));
    }

    partial void OnSelectedSpecOptionChanged(SpecOption value)
    {
        (_syncing, var shouldRebuild) = (true, !_syncing);
        EnsureSecondarySeedDefault();
        RebuildScheme(shouldRebuild);
    }

    partial void OnSelectedSchemeOptionChanged(SchemeOption? value)
    {
        (_syncing, var shouldRebuild) = (true, !_syncing);
        RefreshSpecOptions();
        EnsureSecondarySeedDefault();
        RebuildScheme(shouldRebuild);
    }

    partial void OnSelectedPlatformOptionChanged(PlatformOption value)
    {
        (_syncing, var shouldRebuild) = (true, !_syncing);
        RebuildScheme(shouldRebuild);
    }

    partial void OnSelectedContrastChanged(double value)
    {
        (_syncing, var shouldRebuild) = (true, !_syncing);
        RebuildScheme(shouldRebuild);
    }

    partial void OnSelectedHctChanged(HctSelection value)
    {
        (_syncing, var shouldRebuild) = (true, !_syncing);
        if (IsSecondarySeedVisible)
            SelectedSecondaryHct = SelectedHct;

        RebuildScheme(shouldRebuild);
    }

    partial void OnSelectedSecondaryHctChanged(HctSelection value)
    {
        (_syncing, var shouldRebuild) = (true, !_syncing);
        RebuildScheme(shouldRebuild);
    }

    private void RefreshSchemeOptions()
    {
        var selectedType = SelectedSchemeOption?.SchemeType ?? typeof(TonalSpotScheme);

        SchemeOptions = AllSchemeOptions.ToArray();
        SelectedSchemeOption =
            SchemeOptions.FirstOrDefault(option => option.SchemeType == selectedType)
            ?? SchemeOptions.First();
    }

    private void RefreshSpecOptions()
    {
        if (SelectedSchemeOption is null)
        {
            SpecOptions = AllSpecOptions.ToArray();
            return;
        }

        var currentVersion = SelectedSpecOption?.Version ?? AllSpecOptions[0].Version;
        var newOptions = AllSpecOptions
            .Select(option => option with
            {
                IsEnabled = SupportsSpec(SelectedSchemeOption, option.Version)
            })
            .ToArray();

        var selectedSpec =
            newOptions.FirstOrDefault(option => option.Version == currentVersion && option.IsEnabled)
            ?? newOptions.FirstOrDefault(option => option.IsEnabled)
            ?? newOptions.First();

        SelectedSpecOption = selectedSpec;
        SpecOptions = newOptions;
    }

    private static bool SupportsSpec(SchemeOption option, ColorSpec.SpecVersion version) => version switch
    {
        ColorSpec.SpecVersion.Spec2021 => option.Has2021,
        ColorSpec.SpecVersion.Spec2025 => option.Has2025,
        ColorSpec.SpecVersion.Spec2026 => option.Has2026,
        _ => SchemeAvailability.Unavailable
    } != SchemeAvailability.Unavailable;

    public bool IsFallback => SelectedSpecVersion switch
    {
        ColorSpec.SpecVersion.Spec2021 => SelectedSchemeOption?.Has2021,
        ColorSpec.SpecVersion.Spec2025 => SelectedSchemeOption?.Has2025,
        ColorSpec.SpecVersion.Spec2026 => SelectedSchemeOption?.Has2026,
        _ => SchemeAvailability.Unavailable
    } == SchemeAvailability.FallbackOnly;

    private void RebuildScheme(bool shouldRebuild)
    {
        if (!shouldRebuild) return;
        
        if (SelectedSchemeOption is null)
            return;

        if (!SupportsSpec(SelectedSchemeOption, SelectedSpecVersion))
            return;

        Scheme?.CustomColors.Clear();
        var scheme = CreateScheme(SelectedSchemeOption, SelectedHct, SelectedSecondaryHct, SelectedSpecVersion,
            SelectedPlatformOption.Platform, SelectedContrast);
        foreach (var entry in CustomColors) scheme.CustomColors.Add(entry.Resource);
        Scheme = scheme;
        foreach (var entry in CustomColors) entry.NotifySchemeChanged();

        _syncing = false;
    }

    private void EnsureSecondarySeedDefault()
    {
        if (!IsSecondarySeedVisible)
            return;

        SelectedSecondaryHct = SelectedHct;
    }

    private static T ConfigureSpec<T>(
        T provider,
        ColorSpec.SpecVersion specVersion,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
        where T : ColorScheme
    {
        provider.SpecVersion = specVersion;
        provider.Platform = platform;
        provider.ContrastLevel = contrastLevel;
        return provider;
    }

    private static readonly Dictionary<Type, Func<Color, Color, ColorScheme>> Schemes = new()
    {
        [typeof(TonalSpotScheme)] = (p, _) => new TonalSpotScheme(p),
        [typeof(ExpressiveScheme)] = (p, _) => new ExpressiveScheme(p),
        [typeof(FidelityScheme)] = (p, _) => new FidelityScheme(p),
        [typeof(ContentScheme)] = (p, _) => new ContentScheme(p),
        [typeof(FruitSaladScheme)] = (p, _) => new FruitSaladScheme(p),
        [typeof(MonochromeScheme)] = (p, _) => new MonochromeScheme(p),
        [typeof(NeutralScheme)] = (p, _) => new NeutralScheme(p),
        [typeof(RainbowScheme)] = (p, _) => new RainbowScheme(p),
        [typeof(VibrantScheme)] = (p, _) => new VibrantScheme(p),
        [typeof(CmfScheme)] = (p, s) => new CmfScheme(p) { SecondaryColor = s },
    };

    private static ColorScheme CreateScheme(
        SchemeOption option,
        HctSelection primaryHctSelection,
        HctSelection secondaryHctSelection,
        ColorSpec.SpecVersion specVersion,
        DynamicScheme.Platform platform,
        double contrastLevel
    )
    {
        var primaryColor = primaryHctSelection.ToHct().ToAvaloniaColor();
        var secondaryColor = secondaryHctSelection.ToHct().ToAvaloniaColor();

        if (!Schemes.TryGetValue(option.SchemeType, out var factory))
            throw new NotSupportedException($"Scheme type {option.SchemeType} is not supported.");

        return ConfigureSpec(factory(primaryColor, secondaryColor), specVersion, platform, contrastLevel);
    }
}

public sealed record SpecOption(string Name, ColorSpec.SpecVersion Version, bool IsEnabled = true);

public sealed record SchemeOption(
    string Name,
    Type SchemeType,
    SchemeAvailability Has2021 = SchemeAvailability.Available,
    SchemeAvailability Has2025 = SchemeAvailability.FallbackOnly,
    SchemeAvailability Has2026 = SchemeAvailability.FallbackOnly,
    bool SupportsSecondSeed = false
);

public sealed record PlatformOption(
    string Name,
    DynamicScheme.Platform Platform
);

public enum SchemeAvailability
{
    Available,
    FallbackOnly,
    Unavailable
}
