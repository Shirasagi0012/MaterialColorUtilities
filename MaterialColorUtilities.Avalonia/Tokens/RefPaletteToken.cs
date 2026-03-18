namespace MaterialColorUtilities.Avalonia.Tokens;

internal readonly record struct RefPaletteTokenKey(RefPaletteToken Palette, byte Tone);

public enum RefPaletteToken
{
    Primary,
    Secondary,
    Tertiary,
    Neutral,
    NeutralVariant,
    Error,
    Custom
}
