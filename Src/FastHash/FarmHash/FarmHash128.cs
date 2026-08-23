using System.Runtime.CompilerServices;
using Genbox.FastHash.CityHash;

namespace Genbox.FastHash.FarmHash;

/// <summary>Provides the 128-bit FarmHash algorithm.</summary>
public static class FarmHash128
{
    // farmhashcc's 128-bit path is CityHash128 v1.1.1.

    /// <summary>Computes the hash of a 64-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 128-bit FarmHash value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input) => CityHash128.ComputeIndex(input);

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 128-bit FarmHash value.</returns>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data) => CityHash128.ComputeHash(data);

    /// <summary>Computes the hash of a byte sequence using a seed.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit FarmHash value.</returns>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, UInt128 seed) => CityHash128.ComputeHash(data, seed);
}