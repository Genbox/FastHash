// C# implementation of the DJB2 hash by Daniel J. Bernstein, which is in the public domain.
// See THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

/// <summary>Provides the 64-bit DJB2 hash algorithm.</summary>
public static class Djb2Hash64
{
    /// <summary>Computes the hash of a 64-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 64-bit DJB2 hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong hash = InitHash;
        hash = (hash << 5) + hash + (input & 0xFF);
        hash = (hash << 5) + hash + ((input >> 8) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 16) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 24) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 32) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 40) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 48) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 56) & 0xFF);
        return hash;
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit DJB2 hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        ulong hash = InitHash;

        for (int i = 0; i < data.Length; i++)
            hash = (hash << 5) + hash + data[i];

        return hash;
    }
}