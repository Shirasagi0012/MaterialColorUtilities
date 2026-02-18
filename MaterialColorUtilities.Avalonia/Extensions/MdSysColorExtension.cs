using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using static MaterialColorUtilities.Avalonia.Helpers.NewHelper;

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
        if (!TokenHelper.IsCustom(token))
            throw new ArgumentException($"The token '{token}' does not support a custom key.", nameof(token));

        Token = token;
        CustomKey = customKey;
    }

    [ConstructorArgument("customKey")] public string? CustomKey { get; set; }

    [ConstructorArgument("token")] public SysColorToken Token { get; set; }
    
    public ThemeVariant? Theme { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var (target, parentStack) = GetContextServices(serviceProvider);

        return ShouldProvideBrush(target)
            ? ProvideSysBrushBinding(target, parentStack, Token, CustomKey,themeVariant: Theme)
            : ProvideSysColorBinding(target, parentStack, Token, CustomKey,themeVariant: Theme);
    }
}
