using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

public static class CityHash128Unsafe
{
    public static unsafe Uint128 ComputeHash(byte* s, uint len)
    {
        return len >= 16 ? CityHash128WithSeed(s + 16, len - 16, new Uint128(Read64(s), Read64(s + 8) + k0)) : CityHash128WithSeed(s, len, new Uint128(k0, k1));
    }

    // A subroutine for CityHash128().  Returns a decent 128-bit hash for strings
    // of any length representable in signed long.  Based on City and Murmur.
    private static unsafe Uint128 CityMurmur(byte* s, uint len, Uint128 seed)
    {
        ulong a = seed.Low;
        ulong b = seed.High;
        ulong c = 0;
        ulong d = 0;
        if (len <= 16)
        {
            a = ShiftMix(a * k1) * k1;
            c = b * k1 + HashLen0to16(s, len);
            d = ShiftMix(a + (len >= 8 ? Read64(s) : c));
        }
        else
        {
            c = HashLen16(Read64(s + len - 8) + k1, a);
            d = HashLen16(b + len, c + Read64(s + len - 16));
            a += d;
            // len > 16 here, so do...while is safe
            do
            {
                a ^= ShiftMix(Read64(s) * k1) * k1;
                a *= k1;
                b ^= a;
                c ^= ShiftMix(Read64(s + 8) * k1) * k1;
                c *= k1;
                d ^= c;
                s += 16;
                len -= 16;
            } while (len > 16);
        }
        a = HashLen16(a, c);
        b = HashLen16(d, b);
        return new Uint128(a ^ b, HashLen16(b, a));
    }

    internal static unsafe Uint128 CityHash128WithSeed(byte* s, uint len, Uint128 seed)
    {
        if (len < 128)
            return CityMurmur(s, len, seed);

        // We expect len >= 128 to be the common case.  Keep 56 bytes of state:
        // v, w, x, y, and z.
        Uint128 v, w;
        ulong x = seed.Low;
        ulong y = seed.High;
        ulong z = len * k1;
        v.Low = RotateRight(y ^ k1, 49) * k1 + Read64(s);
        v.High = RotateRight(v.Low, 42) * k1 + Read64(s + 8);
        w.Low = RotateRight(y + z, 35) * k1 + x;
        w.High = RotateRight(x + Read64(s + 88), 53) * k1;

        // This is the same inner loop as CityHash64(), manually unrolled.
        do
        {
            x = RotateRight(x + y + v.Low + Read64(s + 8), 37) * k1;
            y = RotateRight(y + v.High + Read64(s + 48), 42) * k1;
            x ^= w.High;
            y += v.Low + Read64(s + 40);
            z = RotateRight(z + w.Low, 33) * k1;
            v = WeakHashLen32WithSeeds(s, v.High * k1, x + w.Low);
            w = WeakHashLen32WithSeeds(s + 32, z + w.High, y + Read64(s + 16));
            Swap(ref z, ref x);
            s += 64;
            x = RotateRight(x + y + v.Low + Read64(s + 8), 37) * k1;
            y = RotateRight(y + v.High + Read64(s + 48), 42) * k1;
            x ^= w.High;
            y += v.Low + Read64(s + 40);
            z = RotateRight(z + w.Low, 33) * k1;
            v = WeakHashLen32WithSeeds(s, v.High * k1, x + w.Low);
            w = WeakHashLen32WithSeeds(s + 32, z + w.High, y + Read64(s + 16));
            Swap(ref z, ref x);
            s += 64;
            len -= 128;
        } while (len >= 128);
        x += RotateRight(v.Low + z, 49) * k0;
        y = y * k0 + RotateRight(w.High, 37);
        z = z * k0 + RotateRight(w.Low, 27);
        w.Low *= 9;
        v.Low *= k0;
        // If 0 < len < 128, hash up to 4 chunks of 32 bytes each from the end of s.
        for (uint tail_done = 0; tail_done < len;)
        {
            tail_done += 32;
            y = RotateRight(x + y, 42) * k0 + v.High;
            w.Low += Read64(s + len - tail_done + 16);
            x = x * k0 + w.Low;
            z += w.High + Read64(s + len - tail_done);
            w.High += v.Low;
            v = WeakHashLen32WithSeeds(s + len - tail_done, v.Low + z, v.High);
            v.Low *= k0;
        }
        // At this point our 56 bytes of state should contain more than
        // enough information for a strong 128-bit hash.  We use two
        // different 56-byte-to-8-byte hashes to get a 16-byte final result.
        x = HashLen16(x, v.Low);
        y = HashLen16(y + z, w.Low);
        return new Uint128(HashLen16(x + v.High, w.High) + y,
            HashLen16(x + w.High, y + v.High));
    }
}