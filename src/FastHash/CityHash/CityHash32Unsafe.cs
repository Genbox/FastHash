using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashConstants;
using static Genbox.FastHash.MurmurHash.MurmurShared;

namespace Genbox.FastHash.CityHash;

public static class CityHash32Unsafe
{
    public static unsafe uint ComputeHash(byte* s, int length)
    {
        uint len = (uint)length;

        if (len <= 24)
            return len <= 12 ? len <= 4 ? Hash32Len0to4(s, len) : Hash32Len5to12(s, len) : Hash32Len13to24(s, len);

        // len > 24
        uint h = len, g = C1 * h, f = g;
        uint a0 = RotateRight(Read32(s + len - 4) * C1, 17) * C2;
        uint a1 = RotateRight(Read32(s + len - 8) * C1, 17) * C2;
        uint a2 = RotateRight(Read32(s + len - 16) * C1, 17) * C2;
        uint a3 = RotateRight(Read32(s + len - 12) * C1, 17) * C2;
        uint a4 = RotateRight(Read32(s + len - 20) * C1, 17) * C2;
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
        uint iters = (len - 1) / 20;
        do
        {
            a0 = RotateRight(Read32(s) * C1, 17) * C2;
            a1 = Read32(s + 4);
            a2 = RotateRight(Read32(s + 8) * C1, 17) * C2;
            a3 = RotateRight(Read32(s + 12) * C1, 17) * C2;
            a4 = Read32(s + 16);
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
            s += 20;
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

    private static unsafe uint Hash32Len13to24(byte* s, uint len)
    {
        uint a = Read32(s - 4 + (len >> 1));
        uint b = Read32(s + 4);
        uint c = Read32(s + len - 8);
        uint d = Read32(s + (len >> 1));
        uint e = Read32(s);
        uint f = Read32(s + len - 4);
        uint h = len;

        return MurmurMix(Mur(f, Mur(e, Mur(d, Mur(c, Mur(b, Mur(a, h)))))));
    }

    private static unsafe uint Hash32Len0to4(byte* s, uint len)
    {
        uint b = 0;
        uint c = 9;
        for (uint i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)s[i];
            b = b * C1 + v;
            c ^= b;
        }
        return MurmurMix(Mur(b, Mur(len, c)));
    }

    private static unsafe uint Hash32Len5to12(byte* s, uint len)
    {
        uint a = len, b = a * 5, c = 9, d = b;
        a += Read32(s);
        b += Read32(s + len - 4);
        c += Read32(s + ((len >> 1) & 4));
        return MurmurMix(Mur(c, Mur(b, Mur(a, d))));
    }
}