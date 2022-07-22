//Ported to C# by Ian Qvist
//Source: https://github.com/google/farmhash

using System.Runtime.CompilerServices;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash32
{
    public static uint ComputeHash(byte[] s)
    {
        uint length = (uint)s.Length;

        if (length <= 24)
            return length <= 12 ? length <= 4 ? Hash32Len0to4(s, length) : Hash32Len5to12(s, length) : Hash32Len13to24(s, length);

        // len > 24
        uint h = length, g = FarmHashConstants.c1 * length, f = g;
        uint a0 = Utilities.RotateRightCheck(Utilities.Read32(s, length - 4) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a1 = Utilities.RotateRightCheck(Utilities.Read32(s, length - 8) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a2 = Utilities.RotateRightCheck(Utilities.Read32(s, length - 16) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a3 = Utilities.RotateRightCheck(Utilities.Read32(s, length - 12) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a4 = Utilities.RotateRightCheck(Utilities.Read32(s, length - 20) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        h ^= a0;
        h = Utilities.RotateRightCheck(h, 19);
        h = h * 5 + 0xe6546b64;
        h ^= a2;
        h = Utilities.RotateRightCheck(h, 19);
        h = h * 5 + 0xe6546b64;
        g ^= a1;
        g = Utilities.RotateRightCheck(g, 19);
        g = g * 5 + 0xe6546b64;
        g ^= a3;
        g = Utilities.RotateRightCheck(g, 19);
        g = g * 5 + 0xe6546b64;
        f += a4;
        f = Utilities.RotateRightCheck(f, 19) + 113;
        uint iters = (length - 1) / 20;
        uint index = 0;
        do
        {
            uint a = Utilities.Read32(s, index);
            uint b = Utilities.Read32(s, index + 4);
            uint c = Utilities.Read32(s, index + 8);
            uint d = Utilities.Read32(s, index + 12);
            uint e = Utilities.Read32(s, index + 16);
            h += a;
            g += b;
            f += c;
            h = FarmHashShared.Mur(d, h) + e;
            g = FarmHashShared.Mur(c, g) + a;
            f = FarmHashShared.Mur(b + e * FarmHashConstants.c1, f) + d;
            f += g;
            g += f;
            index += 20;
        } while (--iters != 0);
        g = Utilities.RotateRightCheck(g, 11) * FarmHashConstants.c1;
        g = Utilities.RotateRightCheck(g, 17) * FarmHashConstants.c1;
        f = Utilities.RotateRightCheck(f, 11) * FarmHashConstants.c1;
        f = Utilities.RotateRightCheck(f, 17) * FarmHashConstants.c1;
        h = Utilities.RotateRightCheck(h + g, 19);
        h = h * 5 + 0xe6546b64;
        h = Utilities.RotateRightCheck(h, 17) * FarmHashConstants.c1;
        h = Utilities.RotateRightCheck(h + f, 19);
        h = h * 5 + 0xe6546b64;
        h = Utilities.RotateRightCheck(h, 17) * FarmHashConstants.c1;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32Len0to4(byte[] s, uint len, uint seed = 0)
    {
        uint b = seed;
        uint c = 9;
        for (int i = 0; i < len; i++)
        {
            byte v = s[i];
            b = b * FarmHashConstants.c1 + v;
            c ^= b;
        }
        return Utilities.FMix(FarmHashShared.Mur(b, FarmHashShared.Mur(len, c)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32Len5to12(byte[] s, uint len, uint seed = 0)
    {
        uint a = len, b = len * 5, c = 9, d = b + seed;
        a += Utilities.Read32(s);
        b += Utilities.Read32(s, len - 4);
        c += Utilities.Read32(s, (len >> 1) & 4);
        return Utilities.FMix(seed ^ FarmHashShared.Mur(c, FarmHashShared.Mur(b, FarmHashShared.Mur(a, d))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32Len13to24(byte[] s, uint len, uint seed = 0)
    {
        uint a = Utilities.Read32(s, (len >> 1) - 4);
        uint b = Utilities.Read32(s, 4);
        uint c = Utilities.Read32(s, len - 8);
        uint d = Utilities.Read32(s, len >> 1);
        uint e = Utilities.Read32(s);
        uint f = Utilities.Read32(s, len - 4);
        uint h = d * FarmHashConstants.c1 + len + seed;
        a = Utilities.RotateRightCheck(a, 12) + f;
        h = FarmHashShared.Mur(c, h) + a;
        a = Utilities.RotateRightCheck(a, 3) + c;
        h = FarmHashShared.Mur(e, h) + a;
        a = Utilities.RotateRightCheck(a + f, 12) + d;
        h = FarmHashShared.Mur(b ^ seed, h) + a;
        return Utilities.FMix(h);
    }
}