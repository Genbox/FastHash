#if NET8_0_OR_GREATER

// C# port of google/cityhash 1.1.1 (4726e30). Copyright (c) 2011 Google, Inc.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.CityHash.CityHashConstants;
using static Genbox.FastHash.CityHash.CityHash64;

namespace Genbox.FastHash.CityHash;

/// <summary>Provides hardware-accelerated 128-bit CityHash CRC functions.</summary>
public static class CityHashCrc128
{
    /// <summary>Gets whether the required CPU intrinsics are available.</summary>
    public static bool IsSupported => CityHashCrc256.IsSupported;

    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        return CityHash128.ComputeIndex(input);
    }

    /// <summary>Computes the hash of data with the default seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        if (data.Length <= 900)
            return CityHash128.ComputeHash(data);

        Span<ulong> result = stackalloc ulong[4];
        CityHashCrc256.ComputeHashCore(data, result);
        return new UInt128(result[3], result[2]);
    }

    /// <summary>Computes the hash of data with a seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, UInt128 seed)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        if (data.Length <= 900)
            return CityHash128.ComputeHash(data, seed);

        Span<ulong> result = stackalloc ulong[4];
        CityHashCrc256.ComputeHashCore(data, result);
        ulong u = seed.High + result[0];
        ulong v = seed.Low + result[1];
        return new UInt128(HashLen16(RotateRight(v, 32), (u * K0) + result[3]), HashLen16(u, v + result[2]));
    }
}
#endif