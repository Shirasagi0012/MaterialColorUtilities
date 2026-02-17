namespace MaterialColorUtilities.Avalonia;

public enum SysColorToken
{
    Background,
    OnBackground,
    Surface,
    SurfaceDim,
    SurfaceBright,
    SurfaceContainerLowest,
    SurfaceContainerLow,
    SurfaceContainer,
    SurfaceContainerHigh,
    SurfaceContainerHighest,
    OnSurface,
    SurfaceVariant,
    OnSurfaceVariant,
    InverseSurface,
    InverseOnSurface,
    Outline,
    OutlineVariant,
    Shadow,
    Scrim,
    SurfaceTint,
    Primary,
    OnPrimary,
    PrimaryContainer,
    OnPrimaryContainer,
    InversePrimary,
    Secondary,
    OnSecondary,
    SecondaryContainer,
    OnSecondaryContainer,
    Tertiary,
    OnTertiary,
    TertiaryContainer,
    OnTertiaryContainer,
    Error,
    OnError,
    ErrorContainer,
    OnErrorContainer,
    PrimaryFixed,
    PrimaryFixedDim,
    OnPrimaryFixed,
    OnPrimaryFixedVariant,
    SecondaryFixed,
    SecondaryFixedDim,
    OnSecondaryFixed,
    OnSecondaryFixedVariant,
    TertiaryFixed,
    TertiaryFixedDim,
    OnTertiaryFixed,
    OnTertiaryFixedVariant,
    Custom,
    OnCustom,
    CustomContainer,
    OnCustomContainer
}

public enum RefPaletteToken
{
    Primary,
    Secondary,
    Tertiary,
    Neutral,
    NeutralVariant,
    Error
}

public static class TokenHelper
{
    public static bool IsCustom(SysColorToken token)
    {
        return token is SysColorToken.Custom
            or SysColorToken.OnCustom
            or SysColorToken.CustomContainer
            or SysColorToken.OnCustomContainer;
    }


    // TODO: custom palette not implemented now, reserved for later.
    public static bool IsCustom(RefPaletteToken token)
    {
        return false;
    }
}