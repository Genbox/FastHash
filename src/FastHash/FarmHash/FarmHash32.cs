using System.Runtime.CompilerServices;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.MurmurHash.MurmurShared;
using static Genbox.FastHash.FarmHash.FarmHashConstants;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash32
{
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        uint length = (uint)data.Length;

        if (length <= 24)
            return length <= 12 ? length <= 4 ? Hash32Len0to4(data, length) : Hash32Len5to12(data, length) : Hash32Len13to24(data, length);

        // len > 24
        uint h = length, g = C1 * length, f = g;
        uint a0 = RotateRight(Read32(data, length - 4) * C1, 17) * C2;
        uint a1 = RotateRight(Read32(data, length - 8) * C1, 17) * C2;
        uint a2 = RotateRight(Read32(data, length - 16) * C1, 17) * C2;
        uint a3 = RotateRight(Read32(data, length - 12) * C1, 17) * C2;
        uint a4 = RotateRight(Read32(data, length - 20) * C1, 17) * C2;
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
        uint iters = (length - 1) / 20;
        uint index = 0;
        do
        {
            uint a = Read32(data, index);
            uint b = Read32(data, index + 4);
            uint c = Read32(data, index + 8);
            uint d = Read32(data, index + 12);
            uint e = Read32(data, index + 16);
            h += a;
            g += b;
            f += c;
            h = Mur(d, h) + e;
            g = Mur(c, g) + a;
            f = Mur(b + e * C1, f) + d;
            f += g;
            g += f;
            index += 20;
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
    public static uint ComputeIndex(uint input)
    {
        uint b = (uint)(sbyte)(input & 0xFF);
        uint c = 9 ^ b;

        uint v2 = (uint)(sbyte)((input >> 8) & 0xFF);
        b = b * C1 + v2;
        c ^= b;

        uint v3 = (uint)(sbyte)((input >> 16) & 0xFF);
        b = b * C1 + v3;
        c ^= b;

        uint v4 = (uint)(sbyte)((input >> 24) & 0xFF);
        b = b * C1 + v4;
        c ^= b;
        return MurmurMix(Mur(b, Mur(4, c)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32Len13to24(ReadOnlySpan<byte> s, uint len, uint seed = 0)
    {
        uint a = Read32(s, (len >> 1) - 4);
        uint b = Read32(s, 4);
        uint c = Read32(s, len - 8);
        uint d = Read32(s, len >> 1);
        uint e = Read32(s);
        uint f = Read32(s, len - 4);
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