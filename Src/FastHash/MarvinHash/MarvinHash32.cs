using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHash.MarvinHash;

/// <summary>Provides the 32-bit Marvin hash algorithm.</summary>
public static class MarvinHash32
{
    /// <summary>Computes the hash of a 32-bit value using the default seeds.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input) => ComputeIndex(input, 0xb79308cd, 0xced93cd5);

    /// <summary>Computes the hash of a 32-bit value.</summary>
    /// <param name="input">The value to hash.</param><param name="seed1">The first hash seed.</param><param name="seed2">The second hash seed.</param><returns>The 32-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, uint seed1, uint seed2)
    {
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
    /// <param name="data">The bytes to hash.</param><returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0xb79308cd, 0xced93cd5);

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param><param name="seed1">The first hash seed.</param><param name="seed2">The second hash seed.</param><returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed1, uint seed2)
    {
        MarvinHash64.ComputeHash(ref MemoryMarshal.GetReference(data), (uint)data.Length, ref seed1, ref seed2);
        return seed1 ^ seed2;
    }
}