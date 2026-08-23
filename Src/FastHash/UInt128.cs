using System.Runtime.InteropServices;

namespace Genbox.FastHash;

/// <summary>Represents an unsigned 128-bit value as low and high 64-bit words.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct UInt128 : IEquatable<UInt128>
{
    /// <summary>Initializes a 128-bit value from its low and high words.</summary>
    /// <param name="low">The low 64 bits.</param>
    /// <param name="high">The high 64 bits.</param>
    public UInt128(ulong low, ulong high)
    {
        Low = low;
        High = high;
    }

    /// <summary>Gets the low 64 bits of the value.</summary>
    public readonly ulong Low;
    /// <summary>Gets the high 64 bits of the value.</summary>
    public readonly ulong High;

    /// <summary>Determines whether this value equals another 128-bit value.</summary>
    /// <param name="other">The value to compare.</param>
    /// <returns><see langword="true"/> when both words are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(UInt128 other) => Low == other.Low && High == other.High;

    /// <summary>Determines whether this value equals the specified object.</summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> when <paramref name="obj"/> is an equal <see cref="UInt128"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is UInt128 other && Equals(other);

    /// <summary>Returns the hash code for this value.</summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(Low, High);

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true" /> when the values are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(UInt128 left, UInt128 right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true" /> when the values differ; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(UInt128 left, UInt128 right) => !left.Equals(right);

    /// <summary>Returns the low and high words as a comma-separated string.</summary>
    /// <returns>A string representation of this value.</returns>
    public override string ToString() => Low + "," + High;
}