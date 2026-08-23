using System.Runtime.CompilerServices;
using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

/// <summary>Provides the 32-bit XOR variant of the DJB2 hash algorithm.</summary>
public static class Djb2AltHash32
{
    /// <summary>Computes the hash of a 32-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 32-bit DJB2 XOR hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = InitHash;
        hash = ((hash << 5) + hash) ^ (input & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 8) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 16) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 24) & 0xFF);
        return hash;
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 32-bit DJB2 XOR hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        uint hash = InitHash;

        for (int i = 0; i < data.Length; i++)
            hash = ((hash << 5) + hash) ^ data[i];

        return hash;
    }
}