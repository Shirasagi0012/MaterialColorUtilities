namespace MaterialColorUtilities.Avalonia.Tokens;

/// <summary>An immutable resource key for an integer tone of a standard or named palette.</summary>
/// <remarks>Custom names compare using <see cref="StringComparer.OrdinalIgnoreCase"/>.
/// The default value is the valid standard Primary palette at tone zero.</remarks>
public readonly struct RefPaletteKey : IEquatable<RefPaletteKey>
{
    public RefPaletteKey(RefPaletteToken palette, byte tone)
    {
        if (!Enum.IsDefined(palette) || palette == RefPaletteToken.Custom)
            throw new ArgumentOutOfRangeException(nameof(palette), palette, "Use the named constructor for a custom palette.");
        if (tone > 100)
            throw new ArgumentOutOfRangeException(nameof(tone), tone, "Tone must be between 0 and 100.");
        Palette = palette;
        Tone = tone;
        CustomName = null;
    }

    public RefPaletteKey(string customName, byte tone)
    {
        ResourceKeyValidation.ValidateName(customName, nameof(customName));
        if (tone > 100)
            throw new ArgumentOutOfRangeException(nameof(tone), tone, "Tone must be between 0 and 100.");
        Palette = RefPaletteToken.Custom;
        Tone = tone;
        CustomName = customName;
    }

    public RefPaletteToken Palette { get; }
    public byte Tone { get; }
    public string? CustomName { get; }

    internal bool IsValid => Tone <= 100 && Enum.IsDefined(Palette) &&
        (Palette == RefPaletteToken.Custom ? ResourceKeyValidation.IsValidName(CustomName) : CustomName is null);

    public bool Equals(RefPaletteKey other) => Palette == other.Palette && Tone == other.Tone &&
        StringComparer.OrdinalIgnoreCase.Equals(CustomName, other.CustomName);
    public override bool Equals(object? obj) => obj is RefPaletteKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Palette, Tone,
        CustomName is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(CustomName));
    public static bool operator ==(RefPaletteKey left, RefPaletteKey right) => left.Equals(right);
    public static bool operator !=(RefPaletteKey left, RefPaletteKey right) => !left.Equals(right);
}
