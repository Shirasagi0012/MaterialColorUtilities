using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;

namespace MaterialColorUtilities.Avalonia;

/// <summary>
/// Resolves a Material 3 system color role against the scheme in scope at the target element.
/// </summary>
public class MdSysColorExtension
{
    public MdSysColorExtension(SysColorToken token)
    {
        Token = token;
    }

    [ConstructorArgument("token")]
    public SysColorToken Token { get; set; }

    /// <summary>
    /// Pins the theme variant. When unset, the role follows the target element's
    /// <c>ActualThemeVariant</c> and re-resolves as it changes.
    /// </summary>
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
        return ColorTokenBinding.SysColor(Token, CustomKey, Theme, ShouldProvideBrush(serviceProvider));
    }

    internal static bool ShouldProvideBrush(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget target
               || MaterialMarkupExtensionHelper.ShouldProvideBrush(target);
    }
}
