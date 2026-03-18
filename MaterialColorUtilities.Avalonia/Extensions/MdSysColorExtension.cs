using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using DesignTokens;
using MaterialColorUtilities.Avalonia.Helpers;

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

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var observable = TokenExtensionHelper<Color, SysColorTokenKey, MaterialColorSchemeHost>.ProvideObservable(
            serviceProvider,
            new TokenKey<Color, SysColorTokenKey>(new SysColorTokenKey(Token)),
            Theme,
            Colors.Transparent);

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target
            && MaterialMarkupExtensionHelper.ShouldProvideBrush(target))
            return new ColorToBrushObservable(observable)
                .ToBinding();

        return observable.ToBinding();
    }
}
