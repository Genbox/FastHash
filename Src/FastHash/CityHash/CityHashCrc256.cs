#if NET8_0_OR_GREATER

// C# port of google/cityhash 1.1.1 (4726e30). Copyright (c) 2011 Google, Inc.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.CityHash.CityHashConstants;
using static Genbox.FastHash.CityHash.CityHash64;

namespace Genbox.FastHash.CityHash;

/// <summary>Provides hardware-accelerated 256-bit CityHash CRC functions.</summary>
public static class CityHashCrc256
{
    /// <summary>Gets whether the required CPU intrinsics are available.</summary>
    public static bool IsSupported => Sse42.X64.IsSupported;

    /// <summary>Computes the hash of data into a four-word result.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="result">The destination span, which must contain at least four words.</param>
    /// <exception cref="ArgumentException"><paramref name="result" /> contains fewer than four words.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static void ComputeHash(ReadOnlySpan<byte> data, Span<ulong> result)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");
        if (result.Length < 4)
            throw new ArgumentException("The result span must contain at least four ulong values.", nameof(result));

        ComputeHashCore(data, result);
    }

    internal static void ComputeHashCore(ReadOnlySpan<byte> data, Span<ulong> result)
    {
        uint length = (uint)data.Length;
        if (length >= 240)
        {
            CityHashCrc256Long(data, length, 0, result);
            return;
        }

        CityHashCrc256Short(data, length, result);
    }

    // Requires length >= 240.
    private static void CityHashCrc256Long(ReadOnlySpan<byte> s, uint length, uint seed, Span<ulong> result)
    {
        int offset = 0;
        ulong a = Read64(s, 56) + K0;
        ulong b = Read64(s, 96) + K0;
        ulong c = result[0] = HashLen16(b, length);
        ulong d = result[1] = (Read64(s, 120) * K0) + length;
        ulong e = Read64(s, 184) + seed;
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
            Chunk(0, s, ref offset);
            Permute3(ref a, ref h, ref c);
            Chunk(33, s, ref offset);
            Permute3(ref a, ref h, ref f);
            Chunk(0, s, ref offset);
            Permute3(ref b, ref h, ref f);
            Chunk(42, s, ref offset);
            Permute3(ref b, ref h, ref d);
            Chunk(0, s, ref offset);
            Permute3(ref b, ref h, ref e);
            Chunk(33, s, ref offset);
            Permute3(ref a, ref h, ref e);
        } while (--iters > 0);

        while (length >= 40)
        {
            Chunk(29, s, ref offset);
            e ^= RotateRight(a, 20);
            h += RotateRight(b, 30);
            g ^= RotateRight(c, 40);
            f += RotateRight(d, 34);
            Permute3(ref c, ref h, ref g);
            length -= 40;
        }

        if (length > 0)
        {
            offset += (int)length - 40;
            Chunk(33, s, ref offset);
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
        void Chunk(byte r, ReadOnlySpan<byte> input, ref int position)
        {
            Permute3(ref x, ref z, ref y);
            b += Read64(input, position);
            c += Read64(input, position + 8);
            d += Read64(input, position + 16);
            e += Read64(input, position + 24);
            f += Read64(input, position + 32);
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
            position += 40;
        }
    }

    // Requires length < 240.
    private static void CityHashCrc256Short(ReadOnlySpan<byte> data, uint length, Span<ulong> result)
    {
        Span<byte> buffer = stackalloc byte[240];
        buffer.Clear();
        if (length > 0)
            data.CopyTo(buffer);

        CityHashCrc256Long(buffer, 240, ~length, result);
    }
}
#endif