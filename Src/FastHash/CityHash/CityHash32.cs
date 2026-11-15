// C# port of google/cityhash 1.1.1 (4726e30). Copyright (c) 2011 Google, Inc.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.CityHash.CityHash64;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

/// <summary>Provides 32-bit CityHash functions.</summary>
public static class CityHash32
{
    /// <summary>Computes the hash of a 32-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint b = (uint)(sbyte)(input & 0xFF);
        uint c = 9 ^ b;

        b = (b * C1) + (uint)(sbyte)((input >> 8) & 0xFF);
        c ^= b;

        b = (b * C1) + (uint)(sbyte)((input >> 16) & 0xFF);
        c ^= b;

        b = (b * C1) + (uint)(sbyte)((input >> 24) & 0xFF);
        c ^= b;
        return AA_xmxmx_Murmur_32(Mur(b, Mur(4, c)));
    }

    /// <summary>Computes the hash of data with the default seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes the hash of data with a seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        uint len = (uint)data.Length;

        if (len <= 4)
            return Hash32Len0to4(data, len, seed);
        if (len <= 12)
            return Hash32Len5to12(data, len, seed);
        if (len <= 24)
            return Hash32Len13to24(data, len, seed);

        return Hash32Long(ref dataReference, len, seed);
    }

    private static uint Hash32Long(ref byte dataReference, uint len, uint seed)
    {
        // len > 24
        uint h = len + seed, g = C1 * h, f = g;
        uint a0 = RotateRight(Read32(ref dataReference, len - 4) * C1, 17) * C2;
        uint a1 = RotateRight(Read32(ref dataReference, len - 8) * C1, 17) * C2;
        uint a2 = RotateRight(Read32(ref dataReference, len - 16) * C1, 17) * C2;
        uint a3 = RotateRight(Read32(ref dataReference, len - 12) * C1, 17) * C2;
        uint a4 = RotateRight(Read32(ref dataReference, len - 20) * C1, 17) * C2;
        h ^= a0;
        h = RotateRight(h, 19);
        h = (h * 5) + 0xe6546b64;
        h ^= a2;
        h = RotateRight(h, 19);
        h = (h * 5) + 0xe6546b64;
        g ^= a1;
        g = RotateRight(g, 19);
        g = (g * 5) + 0xe6546b64;
        g ^= a3;
        g = RotateRight(g, 19);
        g = (g * 5) + 0xe6546b64;
        f += a4;
        f = RotateRight(f, 19);
        f = (f * 5) + 0xe6546b64;
        uint iters = (len - 1) / 20;
        uint offset = 0;

        do
        {
            a0 = RotateRight(Read32(ref dataReference, offset) * C1, 17) * C2;
            a1 = Read32(ref dataReference, offset + 4);
            a2 = RotateRight(Read32(ref dataReference, offset + 8) * C1, 17) * C2;
            a3 = RotateRight(Read32(ref dataReference, offset + 12) * C1, 17) * C2;
            a4 = Read32(ref dataReference, offset + 16);
            h ^= a0;
            h = RotateRight(h, 18);
            h = (h * 5) + 0xe6546b64;
            f += a1;
            f = RotateRight(f, 19);
            f = f * C1;
            g += a2;
            g = RotateRight(g, 18);
            g = (g * 5) + 0xe6546b64;
            h ^= a3 + a1;
            h = RotateRight(h, 19);
            h = (h * 5) + 0xe6546b64;
            g ^= a4;
            g = ByteSwap(g) * 5;
            h += a4 * 5;
            h = ByteSwap(h);
            f += a0;
            Permute3(ref f, ref h, ref g);
            offset += 20;
        } while (--iters != 0);

        g = RotateRight(g, 11) * C1;
        g = RotateRight(g, 17) * C1;
        f = RotateRight(f, 11) * C1;
        f = RotateRight(f, 17) * C1;
        h = RotateRight(h + g, 19);
        h = (h * 5) + 0xe6546b64;
        h = RotateRight(h, 17) * C1;
        h = RotateRight(h + f, 19);
        h = (h * 5) + 0xe6546b64;
        h = RotateRight(h, 17) * C1;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Hash32Len0to4(ReadOnlySpan<byte> s, uint len)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        uint b = 0;
        uint c = 9;

        for (int i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)Unsafe.Add(ref sReference, i);
            b = (b * C1) + v;
            c ^= b;
        }

        return AA_xmxmx_Murmur_32(Mur(b, Mur(len, c)));
    }

    private static uint Hash32Len0to4(ReadOnlySpan<byte> s, uint len, uint seed)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        uint b = seed;
        uint c = 9;

        for (int i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)Unsafe.Add(ref sReference, i);
            b = (b * C1) + v;
            c ^= b;
        }

        return AA_xmxmx_Murmur_32(Mur(b, Mur(len, c)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Hash32Len5to12(ReadOnlySpan<byte> s, uint len)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        uint a = len, b = a * 5, c = 9, d = b;
        a += Read32(ref sReference);
        b += Read32(ref sReference, len - 4);
        c += Read32(ref sReference, (len >> 1) & 4);
        return AA_xmxmx_Murmur_32(Mur(c, Mur(b, Mur(a, d))));
    }

    private static uint Hash32Len5to12(ReadOnlySpan<byte> s, uint len, uint seed)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        uint a = len + seed, b = a * 5, c = 9, d = b;
        a += Read32(ref sReference);
        b += Read32(ref sReference, len - 4);
        c += Read32(ref sReference, (len >> 1) & 4);
        return AA_xmxmx_Murmur_32(Mur(c, Mur(b, Mur(a, d))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Hash32Len13to24(ReadOnlySpan<byte> s, uint len)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        uint a = Read32(ref sReference, (len >> 1) - 4);
        uint b = Read32(ref sReference, 4);
        uint c = Read32(ref sReference, len - 8);
        uint d = Read32(ref sReference, len >> 1);
        uint e = Read32(ref sReference);
        uint f = Read32(ref sReference, len - 4);
        uint h = len;

        return AA_xmxmx_Murmur_32(Mur(f, Mur(e, Mur(d, Mur(c, Mur(b, Mur(a, h)))))));
    }

    private static uint Hash32Len13to24(ReadOnlySpan<byte> s, uint len, uint seed)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        uint a = Read32(ref sReference, (len >> 1) - 4);
        uint b = Read32(ref sReference, 4);
        uint c = Read32(ref sReference, len - 8);
        uint d = Read32(ref sReference, len >> 1);
        uint e = Read32(ref sReference);
        uint f = Read32(ref sReference, len - 4);
        uint h = len + seed;

        return AA_xmxmx_Murmur_32(Mur(f, Mur(e, Mur(d, Mur(c, Mur(b, Mur(a, h)))))));
    }
}