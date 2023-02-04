using Genbox.FastHash.CityHash;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.FarmHash.FarmHashConstants;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash32Unsafe
{
    // farmhashmk is a seeded version of CityHash
    // farmhashcc is a non-seeded version of CityHash
    // The non-seeded version is a slightly modified version of CityHash for inputs larger than 24, and identical with CityHash on lengths less than 24.

    public static unsafe uint ComputeHash(byte* data, int length, uint seed)
    {
        uint len = (uint)length;

        if (len <= 24)
        {
            if (len >= 13) return Hash32Len13to24(data, len, seed * C1);
            if (len >= 5) return Hash32Len5to12(data, len, seed);
            return Hash32Len0to4(data, len, seed);
        }
        uint h = Hash32Len13to24(data, 24, seed ^ len);
        return Mur(ComputeHash(data + 24, length - 24) + seed, h);
    }

    public static unsafe uint ComputeHash(byte* data, int length)
    {
        uint len = (uint)length;

        if (len <= 24)
            return len <= 12 ? len <= 4 ? CityHash32Unsafe.Hash32Len0to4(data, len) : CityHash32Unsafe.Hash32Len5to12(data, len) : CityHash32Unsafe.Hash32Len13to24(data, len);

        // len > 24
        uint h = len, g = C1 * len, f = g;
        {
            uint a0 = RotateRight(Read32(data + len - 4) * C1, 17) * C2;
            uint a1 = RotateRight(Read32(data + len - 8) * C1, 17) * C2;
            uint a2 = RotateRight(Read32(data + len - 16) * C1, 17) * C2;
            uint a3 = RotateRight(Read32(data + len - 12) * C1, 17) * C2;
            uint a4 = RotateRight(Read32(data + len - 20) * C1, 17) * C2;
            h ^= a0;
            h = RotateRight(h, 19);
            h = h * 5 + 0xe6546b64;
            h ^= a2;
            h = RotateRight(h, 19);
            h = h * 5 + 0xe6546b64;
            g ^= a1;
            g = RotateRight(g, 19);
            g = g * 5 + 0xe6546b64;
            g ^= a3;
            g = RotateRight(g, 19);
            g = g * 5 + 0xe6546b64;
            f += a4;
            f = RotateRight(f, 19);
            f = f * 5 + 0xe6546b64;
        }
        uint iters = (len - 1) / 20;
        do
        {
            uint a0 = RotateRight(Read32(data) * C1, 17) * C2;
            uint a1 = Read32(data + 4);
            uint a2 = RotateRight(Read32(data + 8) * C1, 17) * C2;
            uint a3 = RotateRight(Read32(data + 12) * C1, 17) * C2;
            uint a4 = Read32(data + 16);
            h ^= a0;
            h = RotateRight(h, 18);
            h = h * 5 + 0xe6546b64;
            f += a1;
            f = RotateRight(f, 19);
            f = f * C1;
            g += a2;
            g = RotateRight(g, 18);
            g = g * 5 + 0xe6546b64;
            h ^= a3 + a1;
            h = RotateRight(h, 19);
            h = h * 5 + 0xe6546b64;
            g ^= a4;
            g = ByteSwap(g) * 5;
            h += a4 * 5;
            h = ByteSwap(h);
            f += a0;
            Permute3(ref f, ref h, ref g);
            data += 20;
        } while (--iters != 0);
        g = RotateRight(g, 11) * C1;
        g = RotateRight(g, 17) * C1;
        f = RotateRight(f, 11) * C1;
        f = RotateRight(f, 17) * C1;
        h = RotateRight(h + g, 19);
        h = h * 5 + 0xe6546b64;
        h = RotateRight(h, 17) * C1;
        h = RotateRight(h + f, 19);
        h = h * 5 + 0xe6546b64;
        h = RotateRight(h, 17) * C1;
        return h;
    }

    private static unsafe uint Hash32Len0to4(byte* s, uint len, uint seed)
    {
        uint b = seed;
        uint c = 9;
        for (uint i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)s[i];
            b = b * C1 + v;
            c ^= b;
        }
        return Murmur_32(Mur(b, Mur(len, c)));
    }

    private static unsafe uint Hash32Len5to12(byte* s, uint len, uint seed)
    {
        uint a = len, b = len * 5, c = 9, d = b + seed;
        a += Read32(s);
        b += Read32(s + len - 4);
        c += Read32(s + ((len >> 1) & 4));
        return Murmur_32(seed ^ Mur(c, Mur(b, Mur(a, d))));
    }

    private static unsafe uint Hash32Len13to24(byte* s, uint len, uint seed)
    {
        uint a = Read32(s - 4 + (len >> 1));
        uint b = Read32(s + 4);
        uint c = Read32(s + len - 8);
        uint d = Read32(s + (len >> 1));
        uint e = Read32(s);
        uint f = Read32(s + len - 4);
        uint h = d * C1 + len + seed;
        a = RotateRight(a, 12) + f;
        h = Mur(c, h) + a;
        a = RotateRight(a, 3) + c;
        h = Mur(e, h) + a;
        a = RotateRight(a + f, 12) + d;
        h = Mur(b ^ seed, h) + a;
        return Murmur_32(h);
    }
}