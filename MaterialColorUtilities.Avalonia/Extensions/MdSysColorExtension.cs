using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia;

public class MdSysColorExtension
{
    public MdSysColorExtension(SysColorToken token)
    {
        Token = token;
    }

    [ConstructorArgument("token")]
    public SysColorToken Token { get; set; }

    public ThemeVariant? Theme { get; set; }

    /// <summary>
    /// Names the <see cref="CustomColor"/> to resolve against. Required for the
    /// <see cref="SysColorToken.Custom"/>, <see cref="SysColorToken.OnCustom"/>,
    /// <see cref="SysColorToken.CustomContainer"/> and <see cref="SysColorToken.OnCustomContainer"/>
    /// tokens, and ignored for every other token.
    /// </summary>
    public string? CustomKey { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var observable = TokenExtensionHelper<Color, SysColorTokenKey, MaterialColorSchemeHost>.ProvideObservable(
            serviceProvider,
            new TokenKey<Color, SysColorTokenKey>(new SysColorTokenKey(Token, CustomKey)),
            Theme,
            Colors.Transparent);

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target
            && MaterialMarkupExtensionHelper.ShouldProvideBrush(target))
            return new ColorToBrushObservable(observable)
                .ToBinding();

        return observable.ToBinding();
    }
}
