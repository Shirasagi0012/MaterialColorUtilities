using Avalonia;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Avalonia.Internal;

namespace MaterialColorUtilities.Avalonia;

public class SysBrushExtension 
{
    public SysBrushExtension()
    {
    }

    public SysBrushExtension(SysColorToken token)
    {
        Token = token;
    }
    
    public SysBrushExtension(SysColorToken token, string customKey)
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

    [ConstructorArgument("token")]
    public SysColorToken Token { get; set; }
    
    [ConstructorArgument("customKey")]
    public string? CustomKey { get; set; }
    
    

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return MaterialColorRuntime.ProvideBrush(serviceProvider, (scheme, theme) => scheme.Resolve(Token, theme));
    }
}
