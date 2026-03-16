using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal sealed class MaterialColorBindingContext(
    object? anchor,
    IResourceProvider? providerAnchor,
    AvaloniaObject? targetObject,
    ThemeVariant? explicitThemeVariant,
    ThemeVariant? dictionaryThemeVariant
)
{
    public object? Anchor { get; } = anchor;

    public IResourceProvider? ProviderAnchor { get; } = providerAnchor;

    public AvaloniaObject? TargetObject { get; } = targetObject;

    public ThemeVariant? ExplicitThemeVariant { get; } = explicitThemeVariant;

    public ThemeVariant? DictionaryThemeVariant { get; } = dictionaryThemeVariant;
}