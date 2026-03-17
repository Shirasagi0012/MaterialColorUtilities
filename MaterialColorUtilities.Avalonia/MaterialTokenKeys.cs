using Avalonia.Media;
using DesignTokens;

namespace MaterialColorUtilities.Avalonia;

internal readonly record struct SysColorTokenKey(SysColorToken Token);

internal readonly record struct RefPaletteTokenKey(RefPaletteToken Palette, byte Tone);
