#if NET8_0_OR_GREATER

// C# port of ogxd/gxhash 3.5.0 (6438a7b). Copyright (c) 2023 Olivier Giniaux.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

/// <summary>Provides the 128-bit GxHash2 hash algorithm.</summary>
public static class Gx2Hash128
{
    /// <summary>Computes the hash of a 64-bit value using the default seed.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input) => ComputeIndex(input, 0);

    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input, long seed)
    {
        Vector128<byte> inputVector = Vector128.Create(input, 0UL).AsByte();
        Vector128<byte> lenVector = Vector128.Add(inputVector, Vector128.Create((byte)sizeof(ulong)));
        Vector128<byte> hash = Gx2HashShared.Compute(lenVector, seed);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }

    /// <summary>Computes the hash of a byte sequence using the default seed.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, long seed)
    {
        Vector128<byte> hash = Gx2HashShared.Compute(data, seed);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }
}
#endif