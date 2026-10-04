// C# port of google/farmhash 1.1.0 (0d859a8). Copyright (c) 2014 Google, Inc.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using Genbox.FastHash.CityHash;

namespace Genbox.FastHash.FarmHash;

/// <summary>Provides unsafe access to the 128-bit FarmHash algorithm.</summary>
public static class FarmHash128Unsafe
{
    // farmhashcc's 128-bit path is CityHash128 v1.1.1.

    /// <summary>Computes the hash of bytes at an unmanaged address.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 128-bit FarmHash value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe UInt128 ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return CityHash128Unsafe.ComputeHash(data, length);
    }

    /// <summary>Computes the hash of bytes at an unmanaged address using a seed.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit FarmHash value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe UInt128 ComputeHash(byte* data, int length, UInt128 seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return CityHash128Unsafe.ComputeHash(data, length, seed);
    }
}