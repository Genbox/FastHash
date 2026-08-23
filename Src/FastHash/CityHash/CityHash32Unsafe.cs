using System.Runtime.CompilerServices;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

/// <summary>Provides pointer-based 32-bit CityHash functions.</summary>
public static class CityHash32Unsafe
{
    /// <summary>Computes the hash of a memory region with the default seed.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, 0);
    }

    /// <summary>Computes the hash of a memory region with a seed.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length, uint seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        uint len = (uint)length;

        if (len <= 4)
            return Hash32Len0to4(data, len, seed);
        if (len <= 12)
            return Hash32Len5to12(data, len, seed);
        if (len <= 24)
            return Hash32Len13to24(data, len, seed);

        // len > 24
        uint h = len + seed, g = C1 * h, f = g;
        uint a0 = RotateRight(Read32((data + len) - 4) * C1, 17) * C2;
        uint a1 = RotateRight(Read32((data + len) - 8) * C1, 17) * C2;
        uint a2 = RotateRight(Read32((data + len) - 16) * C1, 17) * C2;
        uint a3 = RotateRight(Read32((data + len) - 12) * C1, 17) * C2;
        uint a4 = RotateRight(Read32((data + len) - 20) * C1, 17) * C2;
        h ^= a0;
        h = RotateRight(h, 19);
        h = (h * 5) + 0xe6546b64;
        h ^= a2;
        h = RotateRight(h, 19);
        h = (h * 5) + 0xe6546b64;
        g ^= a1;
        g = RotateRight(g, 19);
        g = (g * 5) + 0xe6546b64;
        g ^= a3;
        g = RotateRight(g, 19);
        g = (g * 5) + 0xe6546b64;
        f += a4;
        f = RotateRight(f, 19);
        f = (f * 5) + 0xe6546b64;
        uint iters = (len - 1) / 20;

        do
        {
            a0 = RotateRight(Read32(data) * C1, 17) * C2;
            a1 = Read32(data + 4);
            a2 = RotateRight(Read32(data + 8) * C1, 17) * C2;
            a3 = RotateRight(Read32(data + 12) * C1, 17) * C2;
            a4 = Read32(data + 16);
            h ^= a0;
            h = RotateRight(h, 18);
            h = (h * 5) + 0xe6546b64;
            f += a1;
            f = RotateRight(f, 19);
            f = f * C1;
            g += a2;
            g = RotateRight(g, 18);
            g = (g * 5) + 0xe6546b64;
            h ^= a3 + a1;
            h = RotateRight(h, 19);
            h = (h * 5) + 0xe6546b64;
            g ^= a4;
            g = ByteSwap(g) * 5;
            h += a4 * 5;
            h = ByteSwap(h);
            f += a0;
            Permute3(ref f, ref h, ref g);
            data += 20;
        } while (--iters != 0);

        g = RotateRight(g, 11) * C1;
        g = RotateRight(g, 17) * C1;
        f = RotateRight(f, 11) * C1;
        f = RotateRight(f, 17) * C1;
        h = RotateRight(h + g, 19);
        h = (h * 5) + 0xe6546b64;
        h = RotateRight(h, 17) * C1;
        h = RotateRight(h + f, 19);
        h = (h * 5) + 0xe6546b64;
        h = RotateRight(h, 17) * C1;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Hash32Len0to4(byte* s, uint len)
    {
        uint b = 0;
        uint c = 9;

        for (int i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)*(s + i);
            b = (b * C1) + v;
            c ^= b;
        }

        return AA_xmxmx_Murmur_32(Mur(b, Mur(len, c)));
    }

    private static unsafe uint Hash32Len0to4(byte* s, uint len, uint seed)
    {
        uint b = seed;
        uint c = 9;

        for (int i = 0; i < len; i++)
        {
            uint v = (uint)(sbyte)*(s + i);
            b = (b * C1) + v;
            c ^= b;
        }

        return AA_xmxmx_Murmur_32(Mur(b, Mur(len, c)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Hash32Len5to12(byte* s, uint len)
    {
        uint a = len, b = len * 5, c = 9, d = b;
        a += Read32(s);
        b += Read32((s + len) - 4);
        c += Read32(s + ((len >> 1) & 4));
        return AA_xmxmx_Murmur_32(Mur(c, Mur(b, Mur(a, d))));
    }

    private static unsafe uint Hash32Len5to12(byte* s, uint len, uint seed)
    {
        uint a = len + seed, b = a * 5, c = 9, d = b;
        a += Read32(s);
        b += Read32((s + len) - 4);
        c += Read32(s + ((len >> 1) & 4));
        return AA_xmxmx_Murmur_32(Mur(c, Mur(b, Mur(a, d))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Hash32Len13to24(byte* s, uint len)
    {
        uint a = Read32((s - 4) + (len >> 1));
        uint b = Read32(s + 4);
        uint c = Read32((s + len) - 8);
        uint d = Read32(s + (len >> 1));
        uint e = Read32(s);
        uint f = Read32((s + len) - 4);
        uint h = len;

        return AA_xmxmx_Murmur_32(Mur(f, Mur(e, Mur(d, Mur(c, Mur(b, Mur(a, h)))))));
    }

    private static unsafe uint Hash32Len13to24(byte* s, uint len, uint seed)
    {
        uint a = Read32((s - 4) + (len >> 1));
        uint b = Read32(s + 4);
        uint c = Read32((s + len) - 8);
        uint d = Read32(s + (len >> 1));
        uint e = Read32(s);
        uint f = Read32((s + len) - 4);
        uint h = len + seed;

        return AA_xmxmx_Murmur_32(Mur(f, Mur(e, Mur(d, Mur(c, Mur(b, Mur(a, h)))))));
    }
}