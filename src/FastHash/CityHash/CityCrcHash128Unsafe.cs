using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

public static class CityCrcHash128Unsafe
{
    public static unsafe Uint128 ComputeHash(byte* s, uint len)
    {
        if (len <= 900)
            return CityHash128Unsafe.ComputeHash(s, len);

        ulong* result = stackalloc ulong[4];
        CityHashCrc256(s, len, result);
        return new Uint128(result[2], result[3]);
    }

    private static unsafe Uint128 CityHashCrc128WithSeed(byte* s, uint len, Uint128 seed)
    {
        if (len <= 900)
            return CityHash128Unsafe.CityHash128WithSeed(s, len, seed);

        ulong* result = stackalloc ulong[4];
        CityHashCrc256(s, len, result);
        ulong u = seed.High + result[0];
        ulong v = seed.Low + result[1];
        return new Uint128(HashLen16(u, v + result[2]), HashLen16(RotateRight(v, 32), u * k0 + result[3]));
    }

    // Requires len >= 240.
    private static unsafe void CityHashCrc256Long(byte* s, uint len, uint seed, ulong* result)
    {
        ulong a = Read64(s + 56) + k0;
        ulong b = Read64(s + 96) + k0;
        ulong c = result[0] = HashLen16(b, len);
        ulong d = result[1] = Read64(s + 120) * k0 + len;
        ulong e = Read64(s + 184) + seed;
        ulong f = 0;
        ulong g = 0;
        ulong h = c + d;
        ulong x = seed;
        ulong y = 0;
        ulong z = 0;

        // 240 bytes of input per iter.
        uint iters = len / 240;
        len -= iters * 240;
        do
        {
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 0);
            Permute3(ref a, ref h, ref c);
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 33);
            Permute3(ref a, ref h, ref f);
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 0);
            Permute3(ref b, ref h, ref f);
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 42);
            Permute3(ref b, ref h, ref d);
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 0);
            Permute3(ref b, ref h, ref e);
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 33);
            Permute3(ref a, ref h, ref e);
        } while (--iters > 0);

        while (len >= 40)
        {
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 29);
            e ^= RotateRight(a, 20);
            h += RotateRight(b, 30);
            g ^= RotateRight(c, 40);
            f += RotateRight(d, 34);
            Permute3(ref c, ref h, ref g);
            len -= 40;
        }
        if (len > 0)
        {
            s = s + len - 40;
            Chunk(ref s, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref x, ref y, ref z, 33);
            e ^= RotateRight(a, 43);
            h += RotateRight(b, 42);
            g ^= RotateRight(c, 41);
            f += RotateRight(d, 40);
        }
        result[0] ^= h;
        result[1] ^= g;
        g += h;
        a = HashLen16(a, g + z);
        x += y << 32;
        b += x;
        c = HashLen16(c, z) + h;
        d = HashLen16(d, e + result[0]);
        g += e;
        h += HashLen16(x, f);
        e = HashLen16(a, d) + g;
        z = HashLen16(b, c) + a;
        y = HashLen16(g, h) + c;
        result[0] = e + z + y + x;
        a = ShiftMix((a + y) * k0) * k0 + b;
        result[1] += a + result[0];
        a = ShiftMix(a * k0) * k0 + c;
        result[2] = a + result[1];
        a = ShiftMix((a + e) * k0) * k0;
        result[3] = a + result[2];
    }

    private static unsafe void Chunk(ref byte* s, ref ulong a, ref ulong b, ref ulong c, ref ulong d, ref ulong e, ref ulong f, ref ulong g, ref ulong h, ref ulong x, ref ulong y, ref ulong z, byte r)
    {
        Permute3(ref x, ref z, ref y);
        b += Read64(s);
        c += Read64(s + 8);
        d += Read64(s + 16);
        e += Read64(s + 24);
        f += Read64(s + 32);
        a += b;
        h += f;
        b += c;
        f += d;
        g += e;
        e += z;
        g += x;
        z = Crc32(z, b + g);
        y = Crc32(y, e + h);
        x = Crc32(x, f + a);
        e = RotateRight(e, r);
        c += e;
        s += 40;
    }

    private static uint Crc32(ulong a, ulong b) => Sse42.Crc32((uint)a, (uint)b); //_mm_crc32_u64

    // Requires len < 240.
    private static unsafe void CityHashCrc256Short(byte* s, uint len, ulong* result)
    {
        byte* buf = stackalloc byte[240];

        for (int i = 0; i < len; i++)
        {
            buf[i] = s[i];
        }

        // memcpy(buf, s, len);
        // memset(buf + len, 0, 240 - len);
        CityHashCrc256Long(buf, 240, ~len, result);
    }

    private static unsafe void CityHashCrc256(byte* s, uint len, ulong* result)
    {
        if (len >= 240)
            CityHashCrc256Long(s, len, 0, result);
        else
            CityHashCrc256Short(s, len, result);
    }
}