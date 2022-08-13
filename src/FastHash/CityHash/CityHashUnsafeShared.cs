using System.Runtime.CompilerServices;
using static Genbox.FastHash.CityHash.CityHashConstants;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.MurmurHash.MurmurShared;

namespace Genbox.FastHash.CityHash;

internal static class CityHashUnsafeShared
{
    internal static unsafe ulong HashLen0to16(byte* s, uint len)
    {
        if (len >= 8)
        {
            ulong mul = K2 + len * 2;
            ulong a = Read64(s) + K2;
            ulong b = Read64(s + len - 8);
            ulong c = RotateRight(b, 37) * mul + a;
            ulong d = (RotateRight(a, 25) + b) * mul;
            return HashLen16(c, d, mul);
        }
        if (len >= 4)
        {
            ulong mul = K2 + len * 2;
            ulong a = Read32(s);
            return HashLen16(len + (a << 3), Read32(s + len - 4), mul);
        }
        if (len > 0)
        {
            byte a = s[0];
            byte b = s[len >> 1];
            byte c = s[len - 1];
            uint y = a + ((uint)b << 8);
            uint z = len + ((uint)c << 2);
            return ShiftMix((y * K2) ^ (z * K0)) * K2;
        }
        return K2;
    }

    // Return a 16-byte hash for s[0] ... s[31], a, and b.  Quick and dirty.
    internal static unsafe Uint128 WeakHashLen32WithSeeds(byte* s, ulong a, ulong b)
    {
        return CityHashShared.WeakHashLen32WithSeeds(Read64(s),
            Read64(s + 8),
            Read64(s + 16),
            Read64(s + 24),
            a,
            b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Hash32Len0to4(byte* s, uint len, uint seed = 0)
    {
        uint b = seed;
        uint c = 9;
        for (int i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)*(s + i);
            b = b * C1 + v;
            c ^= b;
        }
        return MurmurMix(Mur(b, Mur(len, c)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Hash32Len5to12(byte* s, uint len, uint seed = 0)
    {
        uint a = len, b = len * 5, c = 9, d = b + seed;
        a += Read32(s);
        b += Read32(s + len - 4);
        c += Read32(s + ((len >> 1) & 4));
        return MurmurMix(seed ^ Mur(c, Mur(b, Mur(a, d))));
    }
}