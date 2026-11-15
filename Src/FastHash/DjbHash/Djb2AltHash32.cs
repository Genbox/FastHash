// C# implementation of the DJB2 hash by Daniel J. Bernstein, which is in the public domain.
// See THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        if (data.Length < 4)
            return ComputeShort(ref dataReference, data.Length);

        uint hash = InitHash;
        int i = 0;
        do
        {
            hash = ((hash << 5) + hash) ^ Unsafe.Add(ref dataReference, i);
            hash = ((hash << 5) + hash) ^ Unsafe.Add(ref dataReference, i + 1);
            hash = ((hash << 5) + hash) ^ Unsafe.Add(ref dataReference, i + 2);
            hash = ((hash << 5) + hash) ^ Unsafe.Add(ref dataReference, i + 3);
            i += 4;
        } while (i <= data.Length - 4);

        if (data.Length - i >= 2)
        {
            hash = ((hash << 5) + hash) ^ Unsafe.Add(ref dataReference, i);
            hash = ((hash << 5) + hash) ^ Unsafe.Add(ref dataReference, i + 1);
            i += 2;
        }

        if (i < data.Length)
            hash = ((hash << 5) + hash) ^ Unsafe.Add(ref dataReference, i);

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ComputeShort(ref byte data, int length)
    {
        if (length == 0)
            return InitHash;

        uint hash = (uint)(InitHash * 33) ^ data;
        if (length == 1)
            return hash;

        hash = ((hash << 5) + hash) ^ Unsafe.Add(ref data, 1);
        if (length == 2)
            return hash;

        return ((hash << 5) + hash) ^ Unsafe.Add(ref data, 2);
    }
}