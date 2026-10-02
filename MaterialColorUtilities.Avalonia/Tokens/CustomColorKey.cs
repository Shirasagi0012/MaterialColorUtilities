namespace MaterialColorUtilities.Avalonia.Tokens;

/// <summary>The four fixed-tone roles generated from a named custom color.</summary>
public enum CustomColorRole
{
    Color,
    OnColor,
    Container,
    OnContainer
}

/// <summary>An immutable native resource key for a named custom color role.</summary>
/// <remarks>Names compare using <see cref="StringComparer.OrdinalIgnoreCase"/>.
/// A default key is invalid and safely misses resource lookup.</remarks>
public readonly struct CustomColorKey : IEquatable<CustomColorKey>
{
    public CustomColorKey(string name, CustomColorRole role)
    {
        ResourceKeyValidation.ValidateName(name, nameof(name));
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown custom color role.");
        Name = name;
        Role = role;
    }

    public string Name { get; }
    public CustomColorRole Role { get; }
    internal bool IsValid => ResourceKeyValidation.IsValidName(Name) && Enum.IsDefined(Role);

    public bool Equals(CustomColorKey other) => Role == other.Role &&
        StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name);
    public override bool Equals(object? obj) => obj is CustomColorKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Role,
        Name is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(Name));
    public static bool operator ==(CustomColorKey left, CustomColorKey right) => left.Equals(right);
    public static bool operator !=(CustomColorKey left, CustomColorKey right) => !left.Equals(right);
}
