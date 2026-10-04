#if NET8_0_OR_GREATER
// C# port of AESNI_Hash from PeterRK/PageBloomFilter (src/aesni-hash.h, ae11846). Copyright (c) 2023, Ruan Kunliang.
// Distributed under the BSD 3-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.AesniHash;

/// <summary>Provides 64-bit AES-NI hash functions.</summary>
public static class AesniHash64
{
    /// <summary>Gets whether the required CPU intrinsics are available.</summary>
    public static bool IsSupported => AesniHash128.IsSupported;

    /// <summary>Computes the hash of a 64-bit value with the default seed.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, 0);

    /// <summary>Computes the hash of a 64-bit value with a seed.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, uint seed)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("AesniHash requires AES, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = AesniHash128.Hash128Len8(input, seed);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }

    /// <summary>Computes the hash of data with the default seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes the hash of data with a seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, uint seed)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("AesniHash requires AES, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = AesniHash128.Hash128(data, seed);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }
}
#endif