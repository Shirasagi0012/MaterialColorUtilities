namespace MaterialColorUtilities.Avalonia.Tokens;

internal readonly record struct RefPaletteTokenKey(RefPaletteToken Palette, byte Tone, string? CustomKey = null);

public enum RefPaletteToken
{
    Primary,
    Secondary,
    Tertiary,
    Neutral,
    NeutralVariant,
    Error,

    /// <summary>
    /// The tonal palette of a named <see cref="CustomColor"/>. Requires a custom key.
    /// </summary>
    Custom
}
