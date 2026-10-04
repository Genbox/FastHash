#if NET8_0_OR_GREATER

// C# port of cmuratori/meow_hash 0.5/calico (b080caa). (C) Copyright 2018 Molly Rocket, Inc.
// Distributed under the zlib license; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.MeowHash;

/// <summary>Provides the low 64 bits of the canonical 128-bit MeowHash result from unmanaged memory.</summary>
public static class MeowHash64Unsafe
{
    /// <summary>Gets whether the required AES, SSE, SSE2, and SSSE3 intrinsics are supported.</summary>
    public static bool IsSupported => MeowHash128Unsafe.IsSupported;

    /// <summary>Computes the hash of an unmanaged byte sequence.</summary>
    /// <param name="data">A pointer to at least <paramref name="len" /> bytes.</param>
    /// <param name="len">The non-negative number of bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">The required hardware intrinsics are unavailable.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="len" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int len)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(len);

        if (!IsSupported)
            throw new PlatformNotSupportedException("MeowHash requires AES, SSE, SSE2, and SSSE3 intrinsics.");

        UInt128 res = MeowHash128Unsafe.ComputeHash(data, len);
        return res.Low;
    }
}
#endif