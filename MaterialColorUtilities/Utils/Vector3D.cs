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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MaterialColorUtilities.Utils;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector3D : IEquatable<Vector3D>
{
    public readonly double X;
    public readonly double Y;
    public readonly double Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public bool Equals(Vector3D other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3D v && Equals(v);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public double this[int axis] =>
        axis switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException()
        };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator +(Vector3D a, double scalar)
    {
        return new Vector3D(a.X + scalar, a.Y + scalar, a.Z + scalar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D a, double scalar)
    {
        return new Vector3D(a.X - scalar, a.Y - scalar, a.Z - scalar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(double scalar, Vector3D a)
    {
        return new Vector3D(scalar - a.X, scalar - a.Y, scalar - a.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(Vector3D v, double scalar)
    {
        return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(double scalar, Vector3D v)
    {
        return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(in Vector3D a, in Vector3D b)
    {
        return new Vector3D(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator /(Vector3D v, double scalar)
    {
        return new Vector3D(v.X / scalar, v.Y / scalar, v.Z / scalar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator /(in Vector3D a, in Vector3D b)
    {
        return new Vector3D(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D v)
    {
        return new Vector3D(-v.X, -v.Y, -v.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(in Vector3D a, in Vector3D b)
    {
        return a.X > b.X && a.Y > b.Y && a.Z > b.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(in Vector3D a, in Vector3D b)
    {
        return a.X < b.X && a.Y < b.Y && a.Z < b.Z;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(in Vector3D a, in double scalar)
    {
        return a.X > scalar && a.Y > scalar && a.Z > scalar;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(in Vector3D a, in double scalar)
    {
        return a.X < scalar && a.Y < scalar && a.Z < scalar;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Pow(double exponent)
    {
        return new Vector3D(Math.Pow(X, exponent), Math.Pow(Y, exponent), Math.Pow(Z, exponent));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Abs()
    {
        return new Vector3D(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Signum()
    {
        return new Vector3D(
            X < 0.0 ? -1.0 : X > 0.0 ? 1.0 : 0.0,
            Y < 0.0 ? -1.0 : Y > 0.0 ? 1.0 : 0.0,
            Z < 0.0 ? -1.0 : Z > 0.0 ? 1.0 : 0.0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Max(double scalar)
    {
        return new Vector3D(Math.Max(X, scalar), Math.Max(Y, scalar), Math.Max(Z, scalar));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Max(Vector3D other)
    {
        return new Vector3D(Math.Max(X, other.X), Math.Max(Y, other.Y), Math.Max(Z, other.Z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Min(double scalar)
    {
        return new Vector3D(Math.Min(X, scalar), Math.Min(Y, scalar), Math.Min(Z, scalar));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Min(Vector3D other)
    {
        return new Vector3D(Math.Min(X, other.X), Math.Min(Y, other.Y), Math.Min(Z, other.Z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Apply(Func<double, double> func)
    {
        return new Vector3D(func(X), func(Y), func(Z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D Apply(Vector3D other, Func<double, double, double> func)
    {
        return new Vector3D(func(X, other.X), func(Y, other.Y), func(Z, other.Z));
    }
}
