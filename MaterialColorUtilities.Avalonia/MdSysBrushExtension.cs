using System;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Avalonia.Internal;

namespace MaterialColorUtilities.Avalonia;

public class MdSysBrushExtension
{
    public MdSysBrushExtension()
    {
    }

    public MdSysBrushExtension(SysColorToken token)
    {
        Token = token;
    }

    public MdSysBrushExtension(SysColorToken token, string customKey)
    {
        if (token is SysColorToken.Custom
            or SysColorToken.CustomContainer
            or SysColorToken.OnCustom
            or SysColorToken.OnCustomContainer)
        {
            Token = token;
            CustomKey = customKey;
        }
        else
        {
            throw new ArgumentException($"The token '{token}' does not support a custom key.", nameof(token));
        }
    }

    [ConstructorArgument("customKey")]
    public string? CustomKey { get; set; }

    [ConstructorArgument("token")]
    public SysColorToken Token { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return MaterialColorRuntime.ProvideSysBrushBinding(serviceProvider, Token, CustomKey);
    }
}
