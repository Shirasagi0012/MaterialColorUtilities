using System;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Avalonia.Internal;

namespace MaterialColorUtilities.Avalonia;

public class MdSysColorExtension
{
    public MdSysColorExtension()
    {
    }

    public MdSysColorExtension(SysColorToken token)
    {
        Token = token;
    }

    public MdSysColorExtension(SysColorToken token, string customKey)
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

    [ConstructorArgument("customKey")] public string? CustomKey { get; set; }

    [ConstructorArgument("token")] public SysColorToken Token { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return MaterialColorRuntime.ProvideSysColorBinding(serviceProvider, Token, CustomKey);
    }
}