using System;
using Avalonia.Markup.Xaml;
using static MaterialColorUtilities.Avalonia.Helpers.MaterialColorHelper;

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
        if (!RequiresCustomKey(token))
            throw new ArgumentException($"The token '{token}' does not support a custom key.", nameof(token));

        Token = token;
        CustomKey = customKey;
    }

    [ConstructorArgument("customKey")] public string? CustomKey { get; set; }

    [ConstructorArgument("token")] public SysColorToken Token { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideSysBrushBinding(serviceProvider, Token, CustomKey);
    }
}