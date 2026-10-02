using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia.Markup;

/// <summary>Looks up a system color through Avalonia's native dynamic resource binding.</summary>
public sealed class MdSysColorExtension
{
    public MdSysColorExtension() { }

    public MdSysColorExtension(SysColorToken token) => Token = token;

    [ConstructorArgument("token")]
    public SysColorToken Token { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (!Enum.IsDefined(Token))
            throw new ArgumentOutOfRangeException(nameof(Token), Token, "Unknown system color token.");

        return new DynamicResourceExtension(Token).ProvideValue(serviceProvider);
    }
}
