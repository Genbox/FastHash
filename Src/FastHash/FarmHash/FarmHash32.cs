﻿using System.Runtime.CompilerServices;
using Genbox.FastHash.CityHash;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.FarmHash.FarmHashConstants;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash32
{
    // farmhashmk is a seeded version of CityHash
    // farmhashcc is a non-seeded version of CityHash
    // The non-seeded version is a slightly modified version of CityHash for inputs larger than 24, and identical with CityHash on lengths less than 24.

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

    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed)
    {
        uint len = (uint)data.Length;

        if (len <= 24)
        {
            if (len >= 13) return Hash32Len13to24(data, len, seed * C1);
            if (len >= 5) return Hash32Len5to12(data, len, seed);
            return Hash32Len0to4(data, len, seed);
        }
        uint h = Hash32Len13to24(data, 24, seed ^ len);
        return Mur(ComputeHash(data.Slice(24)) + seed, h);
    }

    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        uint len = (uint)data.Length;

        if (len <= 24)
            return len <= 12 ? len <= 4 ? CityHash32.Hash32Len0to4(data, len) : CityHash32.Hash32Len5to12(data, len) : CityHash32.Hash32Len13to24(data, len);

        // len > 24
        uint h = len, g = C1 * len, f = g;
        {
            uint a0 = RotateRight(Read32(data, len - 4) * C1, 17) * C2;
            uint a1 = RotateRight(Read32(data, len - 8) * C1, 17) * C2;
            uint a2 = RotateRight(Read32(data, len - 16) * C1, 17) * C2;
            uint a3 = RotateRight(Read32(data, len - 12) * C1, 17) * C2;
            uint a4 = RotateRight(Read32(data, len - 20) * C1, 17) * C2;
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
        }
        uint iters = (len - 1) / 20;
        int offset = 0;
        do
        {
            uint a0 = RotateRight(Read32(data, offset) * C1, 17) * C2;
            uint a1 = Read32(data, offset + 4);
            uint a2 = RotateRight(Read32(data, offset + 8) * C1, 17) * C2;
            uint a3 = RotateRight(Read32(data, offset + 12) * C1, 17) * C2;
            uint a4 = Read32(data, offset + 16);
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

    private static uint Hash32Len0to4(ReadOnlySpan<byte> s, uint len, uint seed)
    {
        uint b = seed;
        uint c = 9;
        for (int i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)s[i];
            b = (b * C1) + v;
            c ^= b;
        }
        return AA_xmxmx_Murmur_32(Mur(b, Mur(len, c)));
    }

    private static uint Hash32Len5to12(ReadOnlySpan<byte> s, uint len, uint seed)
    {
        uint a = len, b = len * 5, c = 9, d = b + seed;
        a += Read32(s);
        b += Read32(s, len - 4);
        c += Read32(s, (len >> 1) & 4);
        return AA_xmxmx_Murmur_32(seed ^ Mur(c, Mur(b, Mur(a, d))));
    }

    private static uint Hash32Len13to24(ReadOnlySpan<byte> s, uint len, uint seed)
    {
        uint a = Read32(s, (len >> 1) - 4);
        uint b = Read32(s, 4);
        uint c = Read32(s, len - 8);
        uint d = Read32(s, len >> 1);
        uint e = Read32(s);
        uint f = Read32(s, len - 4);
        uint h = (d * C1) + len + seed;
        a = RotateRight(a, 12) + f;
        h = Mur(c, h) + a;
        a = RotateRight(a, 3) + c;
        h = Mur(e, h) + a;
        a = RotateRight(a + f, 12) + d;
        h = Mur(b ^ seed, h) + a;
        return AA_xmxmx_Murmur_32(h);
    }
}