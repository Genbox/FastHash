// C# port of Cyan4973/xxHash 0.8.3 (e626a72). Copyright (c) 2012-2021 Yann Collet.
// Distributed under the BSD 2-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.XxHash.XxHashConstants;

namespace Genbox.FastHash.XxHash;

/// <summary>Computes 32-bit xxHash hashes.</summary>
public static class XxHash32
{
    /// <summary>Computes a hash for a 32-bit index.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint acc = input * PRIME32_3;
        acc += PRIME32_5;
        acc += 4;

        uint h32 = RotateLeft(acc, 17) * PRIME32_4;
        h32 ^= h32 >> 15;
        h32 *= PRIME32_2;
        h32 ^= h32 >> 13;
        h32 *= PRIME32_3;
        h32 ^= h32 >> 16;
        return h32;
    }

    /// <summary>Computes a hash for the supplied data using a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a hash for the supplied data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        uint len = (uint)data.Length;
        uint h32;
        int offset = 0;

        if (len >= 16)
        {
            ReadOnlySpan<byte> remaining = data;
            uint v1 = seed + PRIME32_1 + PRIME32_2;
            uint v2 = seed + PRIME32_2;
            uint v3 = seed + 0;
            uint v4 = seed - PRIME32_1;

            do
            {
                v1 = Round(v1, Read32(ref MemoryMarshal.GetReference(remaining)));
                v2 = Round(v2, Read32(ref MemoryMarshal.GetReference(remaining), 4));
                v3 = Round(v3, Read32(ref MemoryMarshal.GetReference(remaining), 8));
                v4 = Round(v4, Read32(ref MemoryMarshal.GetReference(remaining), 12));
                remaining = remaining.Slice(16);
            } while (remaining.Length >= 16);

            offset = data.Length - remaining.Length;

            h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
        }
        else
            h32 = seed + PRIME32_5;

        h32 += len;
        len &= 15;

        if (len >= 8)
        {
            h32 = MixWord(h32, Read32(ref dataReference, offset));
            h32 = MixWord(h32, Read32(ref dataReference, offset + 4));
            offset += 8;
            if (len >= 12)
            {
                h32 = MixWord(h32, Read32(ref dataReference, offset));
                offset += 4;
            }
        }
        else if (len >= 4)
        {
            h32 = MixWord(h32, Read32(ref dataReference, offset));
            offset += 4;
        }
        len &= 3;

        if (len > 0)
        {
            h32 = MixByte(h32, ReadByteUnchecked(data, offset));
            if (len > 1)
            {
                h32 = MixByte(h32, ReadByteUnchecked(data, offset + 1));
                if (len > 2)
                    h32 = MixByte(h32, ReadByteUnchecked(data, offset + 2));
            }
        }

        return YC_xmxmx_XXH_32(h32);
    }

    // The tail's length gate proves this byte is within the input.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReadByteUnchecked(ReadOnlySpan<byte> data, int offset) => Unsafe.Add(ref MemoryMarshal.GetReference(data), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixByte(uint hash, byte input) => RotateLeft(hash + (input * PRIME32_5), 11) * PRIME32_1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixWord(uint hash, uint input) => RotateLeft(hash + (input * PRIME32_3), 17) * PRIME32_4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Round(uint seed, uint input)
    {
        seed += input * PRIME32_2;
        seed = RotateLeft(seed, 13);
        seed *= PRIME32_1;
        return seed;
    }
}