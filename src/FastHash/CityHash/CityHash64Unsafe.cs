using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashUnsafeShared;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

public static class CityHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        uint len = (uint)length;
        return CityHash64(data, len);
    }

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed)
    {
        uint len = (uint)length;
        return CityHash64WithSeeds(data, len, K2, seed);
    }

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1, ulong seed2)
    {
        uint len = (uint)length;
        return CityHash64WithSeeds(data, len, seed1, seed2);
    }

    private static unsafe ulong CityHash64(byte* s, uint len)
    {
        if (len <= 32)
        {
            if (len <= 16)
                return HashLen0to16(s, len);
            return HashLen17to32(s, len);
        }
        if (len <= 64)
            return HashLen33to64(s, len);

        // For strings over 64 bytes we hash the end first, and then as we
        // loop we keep 56 bytes of state: v, w, x, y, and z.
        ulong x = Read64(s + len - 40);
        ulong y = Read64(s + len - 16) + Read64(s + len - 56);
        ulong z = City_64_Seed(Read64(s + len - 48) + len, Read64(s + len - 24));
        UInt128 v = WeakHashLen32WithSeeds(s + len - 64, len, z);
        UInt128 w = WeakHashLen32WithSeeds(s + len - 32, y + K1, x);
        x = x * K1 + Read64(s);

        // Decrease len to the nearest multiple of 64, and operate on 64-byte chunks.
        len = (len - 1) & ~63u;
        do
        {
            x = RotateRight(x + y + v.Low + Read64(s + 8), 37) * K1;
            y = RotateRight(y + v.High + Read64(s + 48), 42) * K1;
            x ^= w.High;
            y += v.Low + Read64(s + 40);
            z = RotateRight(z + w.Low, 33) * K1;
            v = WeakHashLen32WithSeeds(s, v.High * K1, x + w.Low);
            w = WeakHashLen32WithSeeds(s + 32, z + w.High, y + Read64(s + 16));
            Swap(ref z, ref x);
            s += 64;
            len -= 64;
        } while (len != 0);
        return City_64_Seed(City_64_Seed(v.Low, w.Low) + ShiftMix(y) * K1 + z, City_64_Seed(v.High, w.High) + x);
    }

    private static unsafe ulong CityHash64WithSeeds(byte* s, uint len, ulong seed0, ulong seed1) => City_64_Seed(CityHash64(s, len) - seed0, seed1);

    // This probably works well for 16-byte strings as well, but it may be overkill in that case.
    private static unsafe ulong HashLen17to32(byte* s, uint len)
    {
        ulong mul = K2 + len * 2;
        ulong a = Read64(s) * K1;
        ulong b = Read64(s + 8);
        ulong c = Read64(s + len - 8) * mul;
        ulong d = Read64(s + len - 16) * K2;
        return City_128_Seed(RotateRight(a + b, 43) + RotateRight(c, 30) + d, a + RotateRight(b + K2, 18) + c, mul);
    }

    // Return an 8-byte hash for 33 to 64 bytes.
    private static unsafe ulong HashLen33to64(byte* s, uint len)
    {
        ulong mul = K2 + len * 2;
        ulong a = Read64(s) * K2;
        ulong b = Read64(s + 8);
        ulong c = Read64(s + len - 24);
        ulong d = Read64(s + len - 32);
        ulong e = Read64(s + 16) * K2;
        ulong f = Read64(s + 24) * 9;
        ulong g = Read64(s + len - 8);
        ulong h = Read64(s + len - 16) * mul;
        ulong u = RotateRight(a + g, 43) + (RotateRight(b, 30) + c) * 9;
        ulong v = ((a + g) ^ d) + f + 1;
        ulong w = ByteSwap((u + v) * mul) + h;
        ulong x = RotateRight(e + f, 42) + c;
        ulong y = (ByteSwap((v + w) * mul) + g) * mul;
        ulong z = e + f + c;
        a = ByteSwap((x + z) * mul + y) + b;
        b = ShiftMix((z + a) * mul + d + h) * mul;
        return b + x;
    }
}