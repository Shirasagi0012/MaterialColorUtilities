namespace MaterialColorUtilities.Avalonia.Tokens;

internal static class ResourceKeyValidation
{
    internal static bool IsValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) &&
        !char.IsWhiteSpace(name[0]) && !char.IsWhiteSpace(name[^1]);

    internal static void ValidateName(string name, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(name, parameterName);
        if (!IsValidName(name))
            throw new ArgumentException("A custom color name must be nonempty and have no leading or trailing whitespace.", parameterName);
    }
}
