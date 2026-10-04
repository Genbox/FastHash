// C# port of Marvin from dotnet/runtime (4017327). Copyright (c) .NET Foundation and Contributors.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHash.MarvinHash;

/// <summary>Provides the 32-bit Marvin hash algorithm.</summary>
public static class MarvinHash32
{
    private const ulong LegacyDefaultSeed = 0xCED93CD5B79308CDUL;

    /// <summary>Computes the hash of a 32-bit value using the default seeds.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input) => ComputeIndex(input, LegacyDefaultSeed);

    /// <summary>Computes the hash of a 32-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed1">The first hash seed.</param>
    /// <param name="seed2">The second hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, uint seed1, uint seed2) => ComputeIndex(input, seed1 | ((ulong)seed2 << 32));

    /// <summary>Computes the hash of a 32-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed">The 64-bit hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, ulong seed)
    {
        uint seed1 = (uint)seed;
        uint seed2 = (uint)(seed >> 32);

        if (!BitConverter.IsLittleEndian)
            input = ByteSwap(input);

        seed1 += input;
        MarvinHash64.Block(ref seed1, ref seed2);
        seed1 += BitConverter.IsLittleEndian ? 0x80u : 0x8000_0000u;
        MarvinHash64.Block(ref seed1, ref seed2);
        MarvinHash64.Block(ref seed1, ref seed2);
        return seed1 ^ seed2;
    }

    /// <summary>Computes the hash of a byte sequence using the default seeds.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, LegacyDefaultSeed);

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed1">The first hash seed.</param>
    /// <param name="seed2">The second hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed1, uint seed2) => ComputeHash(data, seed1 | ((ulong)seed2 << 32));

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The 64-bit hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data, ulong seed)
    {
        uint seed1 = (uint)seed;
        uint seed2 = (uint)(seed >> 32);
        MarvinHash64.ComputeHash(ref MemoryMarshal.GetReference(data), (uint)data.Length, ref seed1, ref seed2);
        return seed1 ^ seed2;
    }
}