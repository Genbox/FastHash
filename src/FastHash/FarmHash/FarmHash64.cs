// Copyright (c) 2014 Google, Inc.
//
// FarmHash, by Geoff Pike
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

//Ported to C# by Ian Qvist
//Source: https://github.com/google/farmhash

namespace Genbox.FastHash.FarmHash;

public static class FarmHash64
{
    private static ulong HashLen16(ulong u, ulong v, ulong mul)
    {
        // Murmur-inspired hashing.
        ulong a = (u ^ v) * mul;
        a ^= a >> 47;
        ulong b = (v ^ a) * mul;
        b ^= b >> 47;
        b *= mul;
        return b;
    }

    private static Uint128 WeakHashLen32WithSeeds(ulong w, ulong x, ulong y, ulong z, ulong a, ulong b)
    {
        a += w;
        b = Utilities.RotateRightCheck(b + a + z, 21);
        ulong c = a;
        a += x;
        a += y;
        b += Utilities.RotateRightCheck(a, 44);
        return new Uint128(a + z, b + c);
    }

    private static Uint128 WeakHashLen32WithSeeds(byte[] data, uint offset, ulong a, ulong b) => WeakHashLen32WithSeeds(Utilities.Fetch64(data, offset),
        Utilities.Fetch64(data, 8 + offset),
        Utilities.Fetch64(data, 16 + offset),
        Utilities.Fetch64(data, 24 + offset),
        a,
        b);

    private static ulong HashLen0to16(byte[] data, uint length)
    {
        if (length >= 8)
        {
            ulong mul = FarmHashConstants.k2 + length * 2;
            ulong a = Utilities.Fetch64(data) + FarmHashConstants.k2;
            ulong b = Utilities.Fetch64(data, length - 8);
            ulong c = Utilities.RotateRightCheck(b, 37) * mul + a;
            ulong d = (Utilities.RotateRightCheck(a, 25) + b) * mul;
            return HashLen16(c, d, mul);
        }
        if (length >= 4)
        {
            ulong mul = FarmHashConstants.k2 + length * 2;
            ulong a = Utilities.Fetch32(data);
            return HashLen16(length + (a << 3), Utilities.Fetch32(data, length - 4), mul);
        }
        if (length > 0)
        {
            byte a = data[0];
            byte b = data[length >> 1];
            byte c = data[length - 1];
            uint y = a + ((uint)b << 8);
            uint z = length + ((uint)c << 2);
            return FarmHash.ShiftMix((y * FarmHashConstants.k2) ^ (z * FarmHashConstants.k0)) * FarmHashConstants.k2;
        }
        return FarmHashConstants.k2;
    }

    private static ulong HashLen17to32(byte[] data, uint length)
    {
        ulong mul = FarmHashConstants.k2 + length * 2;
        ulong a = Utilities.Fetch64(data) * FarmHashConstants.k1;
        ulong b = Utilities.Fetch64(data, 8);
        ulong c = Utilities.Fetch64(data, length - 8) * mul;
        ulong d = Utilities.Fetch64(data, length - 16) * FarmHashConstants.k2;
        return HashLen16(Utilities.RotateRightCheck(a + b, 43) + Utilities.RotateRightCheck(c, 30) + d, a + Utilities.RotateRightCheck(b + FarmHashConstants.k2, 18) + c, mul);
    }

    private static ulong H(ulong x, ulong y, ulong mul, byte r)
    {
        ulong a = (x ^ y) * mul;
        a ^= a >> 47;
        ulong b = (y ^ a) * mul;
        return Utilities.RotateRightCheck(b, r) * mul;
    }

    private static ulong H32(byte[] data, uint offset, uint length, ulong mul, ulong seed0 = 0, ulong seed1 = 0)
    {
        ulong a = Utilities.Fetch64(data, offset) * FarmHashConstants.k1;
        ulong b = Utilities.Fetch64(data, 8 + offset);
        ulong c = Utilities.Fetch64(data, length - 8 + offset) * mul;
        ulong d = Utilities.Fetch64(data, length - 16 + offset) * FarmHashConstants.k2;
        ulong u = Utilities.RotateRightCheck(a + b, 43) + Utilities.RotateRightCheck(c, 30) + d + seed0;
        ulong v = a + Utilities.RotateRightCheck(b + FarmHashConstants.k2, 18) + c + seed1;
        a = FarmHash.ShiftMix((u ^ v) * mul);
        b = FarmHash.ShiftMix((v ^ a) * mul);
        return b;
    }

    private static ulong HashLen33to64(byte[] data, uint length)
    {
        const ulong mul0 = FarmHashConstants.k2 - 30;
        ulong mul1 = FarmHashConstants.k2 - 30 + 2 * length;
        ulong h0 = H32(data, 0, 32, mul0);
        ulong h1 = H32(data, length - 32, 32, mul1);
        return (h1 * mul1 + h0) * mul1;
    }

    private static ulong HashLen65to96(byte[] data, uint length)
    {
        const ulong mul0 = FarmHashConstants.k2 - 114;
        ulong mul1 = FarmHashConstants.k2 - 114 + 2 * length;
        ulong h0 = H32(data, 0, 32, mul0);
        ulong h1 = H32(data, 32, 32, mul1);
        ulong h2 = H32(data, length - 32, 32, mul1, h0, h1);
        return (h2 * 9 + (h0 >> 17) + (h1 >> 21)) * mul1;
    }

    private static ulong Hash64WithSeeds(byte[] s, uint len, ulong seed0, ulong seed1)
    {
        if (len <= 64)
            return HashLen16(Hash64(s, len) - seed0, seed1, 0x9ddfea08eb382d69UL); //PORT NOTE: This used to refer to Hash128to64, which was the same as HashLen16, just with hardcoded mul

        // For strings over 64 bytes we loop.  Internal state consists of
        // 64 bytes: u, v, w, x, y, and z.
        ulong x = seed0;
        ulong y = seed1 * FarmHashConstants.k2 + 113;
        ulong z = FarmHash.ShiftMix(y * FarmHashConstants.k2) * FarmHashConstants.k2;
        Uint128 v = new Uint128(seed0, seed1);
        Uint128 w = new Uint128(0, 0);
        ulong u = x - z;
        x *= FarmHashConstants.k2;
        ulong mul = FarmHashConstants.k2 + (u & 0x82);

        // Set end so that after the loop we have 1 to 64 bytes left to process.
        uint index = 0;
        uint end = (len - 1) / 64 * 64;
        uint last64 = end + ((len - 1) & 63) - 63;
        do
        {
            ulong a0 = Utilities.Fetch64(s);
            ulong a1 = Utilities.Fetch64(s, 8);
            ulong a2 = Utilities.Fetch64(s, 16);
            ulong a3 = Utilities.Fetch64(s, 24);
            ulong a4 = Utilities.Fetch64(s, 32);
            ulong a5 = Utilities.Fetch64(s, 40);
            ulong a6 = Utilities.Fetch64(s, 48);
            ulong a7 = Utilities.Fetch64(s, 56);
            x += a0 + a1;
            y += a2;
            z += a3;
            v.Low += a4;
            v.High += a5 + a1;
            w.Low += a6;
            w.High += a7;

            x = Utilities.RotateRightCheck(x, 26);
            x *= 9;
            y = Utilities.RotateRightCheck(y, 29);
            z *= mul;
            v.Low = Utilities.RotateRightCheck(v.Low, 33);
            v.High = Utilities.RotateRightCheck(v.High, 30);
            w.Low ^= x;
            w.Low *= 9;
            z = Utilities.RotateRightCheck(z, 32);
            z += w.High;
            w.High += z;
            z *= 9;
            Utilities.Swap(ref u, ref y);

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
            w.High = Utilities.RotateRightCheck(w.High, 34);
            Utilities.Swap(ref u, ref z);
            index += 64;
        } while (index != end);
        // Make s point to the last 64 bytes of input.
        index = last64;
        u *= 9;
        v.High = Utilities.RotateRightCheck(v.High, 28);
        v.Low = Utilities.RotateRightCheck(v.Low, 20);
        w.Low += (len - 1) & 63;
        u += y;
        y += u;
        x = Utilities.RotateRightCheck(y - x + v.Low + Utilities.Fetch64(s, index + 8), 37) * mul;
        y = Utilities.RotateRightCheck(y ^ v.High ^ Utilities.Fetch64(s, index + 48), 42) * mul;
        x ^= w.High * 9;
        y += v.Low + Utilities.Fetch64(s, index + 40);
        z = Utilities.RotateRightCheck(z + w.Low, 33) * mul;
        v = WeakHashLen32WithSeeds(s, index + 0, v.High * mul, x + w.Low);
        w = WeakHashLen32WithSeeds(s, index + 32, z + w.High, y + Utilities.Fetch64(s, index + 16));
        return H(HashLen16(v.Low + x, w.Low ^ y, mul) + z - u,
            H(v.High + y, w.High + z, FarmHashConstants.k2, 30) ^ x,
            FarmHashConstants.k2,
            31);
    }

    private static ulong Hash64(byte[] s, uint len)
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
        ulong y = unchecked(seed * FarmHashConstants.k1) + 113;
        ulong z = FarmHash.ShiftMix(y * FarmHashConstants.k2 + 113) * FarmHashConstants.k2;
        Uint128 v = new Uint128(0, 0);
        Uint128 w = new Uint128(0, 0);
        x = x * FarmHashConstants.k2 + Utilities.Fetch64(s);

        // Set end so that after the loop we have 1 to 64 bytes left to process.
        uint index = 0;
        uint end = (len - 1) / 64 * 64;
        uint last64 = end + ((len - 1) & 63) - 63;
        do
        {
            x = Utilities.RotateRightCheck(x + y + v.Low + Utilities.Fetch64(s, 8), 37) * FarmHashConstants.k1;
            y = Utilities.RotateRightCheck(y + v.High + Utilities.Fetch64(s, 48), 42) * FarmHashConstants.k1;
            x ^= w.High;
            y += v.Low + Utilities.Fetch64(s, 40);
            z = Utilities.RotateRightCheck(z + w.Low, 33) * FarmHashConstants.k1;
            v = WeakHashLen32WithSeeds(s, 0, v.High * FarmHashConstants.k1, x + w.Low);
            w = WeakHashLen32WithSeeds(s, 32, z + w.High, y + Utilities.Fetch64(s, 16));
            Utilities.Swap(ref z, ref x);
            index += 64;
        } while (index != end);

        ulong mul = FarmHashConstants.k1 + ((z & 0xff) << 1);
        // Make s point to the last 64 bytes of input.
        index = last64;
        w.Low += (len - 1) & 63;
        v.Low += w.Low;
        w.Low += v.Low;
        x = Utilities.RotateRightCheck(x + y + v.Low + Utilities.Fetch64(s, index + 8), 37) * mul;
        y = Utilities.RotateRightCheck(y + v.High + Utilities.Fetch64(s, index + 48), 42) * mul;
        x ^= w.High * 9;
        y += v.Low * 9 + Utilities.Fetch64(s, index + 40);
        z = Utilities.RotateRightCheck(z + w.Low, 33) * mul;
        v = WeakHashLen32WithSeeds(s, index + 0, v.High * mul, x + w.Low);
        w = WeakHashLen32WithSeeds(s, index + 32, z + w.High, y + Utilities.Fetch64(s, index + 16));
        Utilities.Swap(ref z, ref x);
        return HashLen16(HashLen16(v.Low, w.Low, mul) + FarmHash.ShiftMix(y) * FarmHashConstants.k0 + z,
            HashLen16(v.High, w.High, mul) + x,
            mul);
    }

    public static ulong ComputeHash(byte[] s, ulong seed0 = 81, ulong seed1 = 0)
    {
        uint len = (uint)s.Length;

        if (len <= 32)
            return len <= 16 ? HashLen0to16(s, len) : HashLen17to32(s, len);

        if (len <= 64)
            return HashLen33to64(s, len);

        if (len <= 96)
            return HashLen65to96(s, len);

        if (len <= 256)
            return Hash64(s, len);

        return Hash64WithSeeds(s, len, seed0, seed1);
    }
}