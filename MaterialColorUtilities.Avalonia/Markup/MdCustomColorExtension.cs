using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia.Markup;

/// <summary>Looks up a named custom color role through Avalonia's native dynamic resource binding.</summary>
public sealed class MdCustomColorExtension
{
    public MdCustomColorExtension() { }

    public MdCustomColorExtension(string name, CustomColorRole role)
    {
        Name = name;
        Role = role;
    }

    [ConstructorArgument("name")]
    public string Name { get; set; } = null!;

    [ConstructorArgument("role")]
    public CustomColorRole Role { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider) =>
        new DynamicResourceExtension(new CustomColorKey(Name, Role)).ProvideValue(serviceProvider);
}
