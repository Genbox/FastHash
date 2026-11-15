// C# implementation of the DJB2 hash by Daniel J. Bernstein, which is in the public domain.
// See THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

/// <summary>Provides the 32-bit DJB2 hash algorithm.</summary>
public static class Djb2Hash32
{
    /// <summary>Computes the hash of a 32-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 32-bit DJB2 hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = InitHash;
        hash = (hash << 5) + hash + (input & 0xFF);
        hash = (hash << 5) + hash + ((input >> 8) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 16) & 0xFF);
        hash = (hash << 5) + hash + ((input >> 24) & 0xFF);
        return hash;
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 32-bit DJB2 hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        uint hash = InitHash;

        int i = 0;
        for (; i <= data.Length - 4; i += 4)
        {
            // Expand four h = h * 33 + byte steps to shorten the dependency chain.
            hash = (hash * 1185921)
                   + ((uint)Unsafe.Add(ref dataReference, i) * 35937)
                   + ((uint)Unsafe.Add(ref dataReference, i + 1) * 1089)
                   + ((uint)Unsafe.Add(ref dataReference, i + 2) * 33)
                   + Unsafe.Add(ref dataReference, i + 3);
        }

        if (data.Length - i >= 2)
        {
            hash = (hash * 1089)
                   + ((uint)Unsafe.Add(ref dataReference, i) * 33)
                   + Unsafe.Add(ref dataReference, i + 1);
            i += 2;
        }

        if (i < data.Length)
            hash = (hash << 5) + hash + Unsafe.Add(ref dataReference, i);

        return hash;
    }
}