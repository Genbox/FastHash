//Ported to C# by Ian Qvist
//Source: https://github.com/google/farmhash

using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.FarmHash.FarmHashConstants;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash64
{
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed0 = 81, ulong seed1 = 0)
    {
        uint len = (uint)data.Length;

        if (len <= 32)
            return len <= 16 ? HashLen0to16(data, len) : HashLen17to32(data, len);

        if (len <= 64)
            return HashLen33to64(data, len);

        if (len <= 96)
            return HashLen65to96(data, len);

        if (len <= 256)
            return Hash64(data, len);

        return Hash64WithSeeds(data, len, seed0, seed1);
    }

    private static ulong HashLen0to16(ReadOnlySpan<byte> data, uint length)
    {
        if (length >= 8)
        {
            ulong mul = K2 + length * 2;
            ulong a = Read64(data) + K2;
            ulong b = Read64(data, length - 8);
            ulong c = RotateRight(b, 37) * mul + a;
            ulong d = (RotateRight(a, 25) + b) * mul;
            return HashLen16(c, d, mul);
        }
        if (length >= 4)
        {
            ulong mul = K2 + length * 2;
            ulong a = Read32(data);
            return HashLen16(length + (a << 3), Read32(data, length - 4), mul);
        }
        if (length > 0)
        {
            byte a = data[0];
            byte b = data[(int)(length >> 1)];
            byte c = data[(int)(length - 1)];
            uint y = a + ((uint)b << 8);
            uint z = length + ((uint)c << 2);
            return ShiftMix((y * K2) ^ (z * K0)) * K2;
        }
        return K2;
    }

    private static ulong HashLen17to32(ReadOnlySpan<byte> data, uint length)
    {
        ulong mul = K2 + length * 2;
        ulong a = Read64(data) * K1;
        ulong b = Read64(data, 8);
        ulong c = Read64(data, length - 8) * mul;
        ulong d = Read64(data, length - 16) * K2;
        return HashLen16(RotateRight(a + b, 43) + RotateRight(c, 30) + d, a + RotateRight(b + K2, 18) + c, mul);
    }

    private static ulong HashLen33to64(ReadOnlySpan<byte> data, uint length)
    {
        const ulong mul0 = K2 - 30;
        ulong mul1 = K2 - 30 + 2 * length;
        ulong h0 = H32(data, 0, 32, mul0);
        ulong h1 = H32(data, length - 32, 32, mul1);
        return (h1 * mul1 + h0) * mul1;
    }

    private static ulong HashLen65to96(ReadOnlySpan<byte> data, uint length)
    {
        const ulong mul0 = K2 - 114;
        ulong mul1 = K2 - 114 + 2 * length;
        ulong h0 = H32(data, 0, 32, mul0);
        ulong h1 = H32(data, 32, 32, mul1);
        ulong h2 = H32(data, length - 32, 32, mul1, h0, h1);
        return (h2 * 9 + (h0 >> 17) + (h1 >> 21)) * mul1;
    }

    private static ulong H(ulong x, ulong y, ulong mul, byte r)
    {
        ulong a = (x ^ y) * mul;
        a ^= a >> 47;
        ulong b = (y ^ a) * mul;
        return RotateRight(b, r) * mul;
    }

    private static ulong H32(ReadOnlySpan<byte> data, uint offset, uint length, ulong mul, ulong seed0 = 0, ulong seed1 = 0)
    {
        ulong a = Read64(data, offset) * K1;
        ulong b = Read64(data, 8 + offset);
        ulong c = Read64(data, length - 8 + offset) * mul;
        ulong d = Read64(data, length - 16 + offset) * K2;
        ulong u = RotateRight(a + b, 43) + RotateRight(c, 30) + d + seed0;
        ulong v = a + RotateRight(b + K2, 18) + c + seed1;
        a = ShiftMix((u ^ v) * mul);
        b = ShiftMix((v ^ a) * mul);
        return b;
    }

    private static Uint128 WeakHashLen32WithSeeds(ulong w, ulong x, ulong y, ulong z, ulong a, ulong b)
    {
        a += w;
        b = RotateRight(b + a + z, 21);
        ulong c = a;
        a += x;
        a += y;
        b += RotateRight(a, 44);
        return new Uint128(a + z, b + c);
    }

    private static Uint128 WeakHashLen32WithSeeds(ReadOnlySpan<byte> data, uint offset, ulong a, ulong b)
    {
        return WeakHashLen32WithSeeds(Read64(data, offset),
            Read64(data, 8 + offset),
            Read64(data, 16 + offset),
            Read64(data, 24 + offset),
            a,
            b);
    }

    private static ulong Hash64WithSeeds(ReadOnlySpan<byte> s, uint len, ulong seed0, ulong seed1)
    {
        if (len <= 64)
            return HashLen16(Hash64(s, len) - seed0, seed1, 0x9ddfea08eb382d69UL); //PORT NOTE: This used to refer to Hash128to64, which was the same as HashLen16, just with hardcoded mul

        // For strings over 64 bytes we loop.  Internal state consists of
        // 64 bytes: u, v, w, x, y, and z.
        ulong x = seed0;
        ulong y = seed1 * K2 + 113;
        ulong z = ShiftMix(y * K2) * K2;
        Uint128 v = new Uint128(seed0, seed1);
        Uint128 w = new Uint128(0, 0);
        ulong u = x - z;
        x *= K2;
        ulong mul = K2 + (u & 0x82);

        // Set end so that after the loop we have 1 to 64 bytes left to process.
        uint index = 0;
        uint end = (len - 1) / 64 * 64;
        uint last64 = end + ((len - 1) & 63) - 63;
        do
        {
            ulong a0 = Read64(s);
            ulong a1 = Read64(s, 8);
            ulong a2 = Read64(s, 16);
            ulong a3 = Read64(s, 24);
            ulong a4 = Read64(s, 32);
            ulong a5 = Read64(s, 40);
            ulong a6 = Read64(s, 48);
            ulong a7 = Read64(s, 56);
            x += a0 + a1;
            y += a2;
            z += a3;
            v.Low += a4;
            v.High += a5 + a1;
            w.Low += a6;
            w.High += a7;

            x = RotateRight(x, 26);
            x *= 9;
            y = RotateRight(y, 29);
            z *= mul;
            v.Low = RotateRight(v.Low, 33);
            v.High = RotateRight(v.High, 30);
            w.Low ^= x;
            w.Low *= 9;
            z = RotateRight(z, 32);
            z += w.High;
            w.High += z;
            z *= 9;
            Swap(ref u, ref y);

            z += a0 + a6;
            v.Low += a2;
            v.High += a3;
            w.Low += a4;
            w.High += a5 + a6;
            x += a1;
            y += a7;

            y += v.Low;
            v.Low += x - y;
            v.High += w.Low;
            w.Low += v.High;
            w.High += x - y;
            x += w.High;
            w.High = RotateRight(w.High, 34);
            Swap(ref u, ref z);
            index += 64;
        } while (index != end);
        // Make s point to the last 64 bytes of input.
        index = last64;
        u *= 9;
        v.High = RotateRight(v.High, 28);
        v.Low = RotateRight(v.Low, 20);
        w.Low += (len - 1) & 63;
        u += y;
        y += u;
        x = RotateRight(y - x + v.Low + Read64(s, index + 8), 37) * mul;
        y = RotateRight(y ^ v.High ^ Read64(s, index + 48), 42) * mul;
        x ^= w.High * 9;
        y += v.Low + Read64(s, index + 40);
        z = RotateRight(z + w.Low, 33) * mul;
        v = WeakHashLen32WithSeeds(s, index + 0, v.High * mul, x + w.Low);
        w = WeakHashLen32WithSeeds(s, index + 32, z + w.High, y + Read64(s, index + 16));
        return H(HashLen16(v.Low + x, w.Low ^ y, mul) + z - u, H(v.High + y, w.High + z, K2, 30) ^ x, K2, 31);
    }

    private static ulong Hash64(ReadOnlySpan<byte> s, uint len)
    {
        const ulong seed = 81;

        if (len <= 32)
        {
            if (len <= 16)
                return HashLen0to16(s, len);

            return HashLen17to32(s, len);
        }

        if (len <= 64)
            return HashLen33to64(s, len);

        // For strings over 64 bytes we loop. Internal state consists of 56 bytes: v, w, x, y, and z.
        ulong x = seed;
        ulong y = unchecked(seed * K1) + 113;
        ulong z = ShiftMix(y * K2 + 113) * K2;
        Uint128 v = Uint128.Zero;
        Uint128 w = Uint128.Zero;
        x = x * K2 + Read64(s);

        // Set end so that after the loop we have 1 to 64 bytes left to process.
        uint index = 0;
        uint end = (len - 1) / 64 * 64;
        uint last64 = end + ((len - 1) & 63) - 63;
        do
        {
            x = RotateRight(x + y + v.Low + Read64(s, 8), 37) * K1;
            y = RotateRight(y + v.High + Read64(s, 48), 42) * K1;
            x ^= w.High;
            y += v.Low + Read64(s, 40);
            z = RotateRight(z + w.Low, 33) * K1;
            v = WeakHashLen32WithSeeds(s, 0, v.High * K1, x + w.Low);
            w = WeakHashLen32WithSeeds(s, 32, z + w.High, y + Read64(s, 16));
            Swap(ref z, ref x);
            index += 64;
        } while (index != end);

        ulong mul = K1 + ((z & 0xff) << 1);
        // Make s point to the last 64 bytes of input.
        index = last64;
        w.Low += (len - 1) & 63;
        v.Low += w.Low;
        w.Low += v.Low;
        x = RotateRight(x + y + v.Low + Read64(s, index + 8), 37) * mul;
        y = RotateRight(y + v.High + Read64(s, index + 48), 42) * mul;
        x ^= w.High * 9;
        y += v.Low * 9 + Read64(s, index + 40);
        z = RotateRight(z + w.Low, 33) * mul;
        v = WeakHashLen32WithSeeds(s, index + 0, v.High * mul, x + w.Low);
        w = WeakHashLen32WithSeeds(s, index + 32, z + w.High, y + Read64(s, index + 16));
        Swap(ref z, ref x);
        return HashLen16(HashLen16(v.Low, w.Low, mul) + ShiftMix(y) * K0 + z, HashLen16(v.High, w.High, mul) + x, mul);
    }
}