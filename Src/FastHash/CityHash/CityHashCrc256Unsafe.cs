#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.CityHash.CityHashConstants;
using static Genbox.FastHash.CityHash.CityHashShared;

namespace Genbox.FastHash.CityHash;

public static class CityHashCrc256Unsafe
{
    public static bool IsSupported => Sse42.X64.IsSupported;

    public static unsafe void ComputeHash(byte* data, int length, ulong* result)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        ComputeHashInternal(data, (uint)length, result);
    }

    internal static unsafe void ComputeHashInternal(byte* data, uint length, ulong* result)
    {
        if (length >= 240)
        {
            CityHashCrc256Long(data, length, 0, result);
            return;
        }

        CityHashCrc256Short(data, length, result);
    }

    // Requires length >= 240.
    private static unsafe void CityHashCrc256Long(byte* s, uint length, uint seed, ulong* result)
    {
        ulong a = Read64(s + 56) + K0;
        ulong b = Read64(s + 96) + K0;
        ulong c = result[0] = HashLen16(b, length);
        ulong d = result[1] = (Read64(s + 120) * K0) + length;
        ulong e = Read64(s + 184) + seed;
        ulong f = 0;
        ulong g = 0;
        ulong h = c + d;
        ulong x = seed;
        ulong y = 0;
        ulong z = 0;

        // 240 bytes of input per iteration.
        uint iters = length / 240;
        length -= iters * 240;
        do
        {
            Chunk(0);
            Permute3(ref a, ref h, ref c);
            Chunk(33);
            Permute3(ref a, ref h, ref f);
            Chunk(0);
            Permute3(ref b, ref h, ref f);
            Chunk(42);
            Permute3(ref b, ref h, ref d);
            Chunk(0);
            Permute3(ref b, ref h, ref e);
            Chunk(33);
            Permute3(ref a, ref h, ref e);
        } while (--iters > 0);

        while (length >= 40)
        {
            Chunk(29);
            e ^= RotateRight(a, 20);
            h += RotateRight(b, 30);
            g ^= RotateRight(c, 40);
            f += RotateRight(d, 34);
            Permute3(ref c, ref h, ref g);
            length -= 40;
        }

        if (length > 0)
        {
            s += (int)length - 40;
            Chunk(33);
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
        a = (ShiftMix((a + y) * K0) * K0) + b;
        result[1] += a + result[0];
        a = (ShiftMix(a * K0) * K0) + c;
        result[2] = a + result[1];
        a = ShiftMix((a + e) * K0) * K0;
        result[3] = a + result[2];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Chunk(byte r)
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
            z = Sse42.X64.Crc32(z, b + g);
            y = Sse42.X64.Crc32(y, e + h);
            x = Sse42.X64.Crc32(x, f + a);
            e = RotateRight(e, r);
            c += e;
            s += 40;
        }
    }

    // Requires length < 240.
    private static unsafe void CityHashCrc256Short(byte* data, uint length, ulong* result)
    {
        Span<byte> buffer = stackalloc byte[240];
        buffer.Clear();
        if (length > 0)
            new ReadOnlySpan<byte>(data, (int)length).CopyTo(buffer);

        fixed (byte* bufferPtr = buffer)
            CityHashCrc256Long(bufferPtr, 240, ~length, result);
    }
}
#endif