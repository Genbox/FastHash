#if NET8_0_OR_GREATER
// C# port of cmuratori/meow_hash 0.5/calico (b080caa). (C) Copyright 2018 Molly Rocket, Inc.
// Distributed under the zlib license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.MeowHash;

/// <summary>Provides the low 64 bits of the canonical 128-bit MeowHash result.</summary>
public static class MeowHash64
{
    /// <summary>Gets whether the required AES, SSE, SSE2, and SSSE3 intrinsics are supported.</summary>
    public static bool IsSupported => MeowHash128.IsSupported;

    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">The required hardware intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("MeowHash requires AES, SSE, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = MeowHash128.HashLen8(input);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">The required hardware intrinsics are unavailable.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        Vector128<byte> res = MeowHash128.ComputeHashVector(data);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }
}
#endif