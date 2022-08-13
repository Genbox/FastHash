//Ported to C# by Ian Qvist
//Source: https://github.com/google/farmhash

using System.Runtime.CompilerServices;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashUnsafeShared;
using static Genbox.FastHash.MurmurHash.MurmurShared;
using static Genbox.FastHash.FarmHash.FarmHashConstants;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash32Unsafe
{
    public static unsafe uint ComputeHash(byte* dataPtr, int length)
    {
        uint len = (uint)length;

        if (len <= 24)
            return len <= 12 ? len <= 4 ? Hash32Len0to4(dataPtr, len) : Hash32Len5to12(dataPtr, len) : Hash32Len13to24(dataPtr, len);

        // len > 24
        uint h = len, g = C1 * len, f = g;
        uint a0 = RotateRight(Read32(dataPtr + len - 4) * C1, 17) * C2;
        uint a1 = RotateRight(Read32(dataPtr + len - 8) * C1, 17) * C2;
        uint a2 = RotateRight(Read32(dataPtr + len - 16) * C1, 17) * C2;
        uint a3 = RotateRight(Read32(dataPtr + len - 12) * C1, 17) * C2;
        uint a4 = RotateRight(Read32(dataPtr + len - 20) * C1, 17) * C2;
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
        f = RotateRight(f, 19) + 113;
        uint iters = (len - 1) / 20;
        do
        {
            uint a = Read32(dataPtr);
            uint b = Read32(dataPtr + 4);
            uint c = Read32(dataPtr + 8);
            uint d = Read32(dataPtr + 12);
            uint e = Read32(dataPtr + 16);
            h += a;
            g += b;
            f += c;
            h = Mur(d, h) + e;
            g = Mur(c, g) + a;
            f = Mur(b + e * C1, f) + d;
            f += g;
            g += f;
            dataPtr += 20;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint Hash32Len13to24(byte* s, uint len, uint seed = 0)
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
        return MurmurMix(h);
    }
}