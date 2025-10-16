// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace MaterialColorUtilities.Utils;

/// <summary>
/// Represents a 32-bit ARGB (alpha, red, green, blue) color value.
/// </summary>
/// <remarks>
/// The ArgbColor struct provides convenient access to the individual alpha, red, green, and blue
/// components of a color, as well as the packed 32-bit integer value. It has the same level of
/// efficiency as using a raw integer, while being significantly easier to use.
/// It is used to replace int-based color representations in the original library.
/// </remarks>
public struct ArgbColor : IEquatable<ArgbColor>
{
    private int _value;
    public int Value => _value;

    public ArgbColor(int value)
    {
        _value = value;
    }

    public ArgbColor(uint value)
    {
        _value = unchecked((int)value);
    }

    public ArgbColor(byte alpha, byte red, byte green, byte blue)
    {
        _value = (alpha << 24) | (red << 16) | (green << 8) | blue;
    }

    public ArgbColor(Vector3D v) : this(255, (byte)v.X, (byte)v.Y, (byte)v.Z)
    {
    }

    public void Deconstruct (out byte alpha, out byte red, out byte green, out byte blue)
    {
        alpha = Alpha;
        red = Red;
        green = Green;
        blue = Blue;
    }

    public byte Alpha
    {
        get => (byte)((_value >> 24) & 0xFF);
        set => _value = (_value & 0x00FFFFFF) | ((value & 0xFF) << 24);
    }

    public byte Red
    {
        get => (byte)((_value >> 16) & 0xFF);
        set => _value = (int)(_value & 0xFF00FFFF) | ((value & 0xFF) << 16);
    }

    public byte Green
    {
        get => (byte)((_value >> 8) & 0xFF);
        set => _value = (int)(_value & 0xFFFF00FF) | ((value & 0xFF) << 8);
    }

    public byte Blue
    {
        get => (byte)(_value & 0xFF);
        set => _value = (int)(_value & 0xFFFFFF00) | (value & 0xFF);
    }

    public bool IsOpaque()
    {
        return Alpha == 255;
    }

    public bool Equals(ArgbColor other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ArgbColor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value;
    }

    public static bool operator ==(ArgbColor left, ArgbColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArgbColor left, ArgbColor right)
    {
        return !(left == right);
    }
}