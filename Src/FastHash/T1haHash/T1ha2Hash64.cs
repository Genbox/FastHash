using System.Runtime.CompilerServices;
using static Genbox.FastHash.T1haHash.T1haHashConstants;
using static Genbox.FastHash.T1haHash.T1haHashShared;

namespace Genbox.FastHash.T1haHash;

/// <summary>Computes 64-bit t1ha2 hashes.</summary>
public static class T1ha2Hash64
{
    /// <summary>Computes a hash for a 64-bit index using a zero seed.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, 0);

    /// <summary>Computes a hash for a 64-bit index.</summary>
    /// <param name="input">The index to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed)
    {
        ulong a = seed;
        ulong b = sizeof(ulong);
        MixUp(ref b, ref a, input, Prime1);
        return Final64(a, b);
    }

    /// <summary>Computes a hash for the supplied data using a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a hash for the supplied data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed)
    {
        int length = data.Length;
        int offset = 0;

        ulong a = seed;
        ulong b = (ulong)length;

        if (length > 32)
        {
            ulong c = RotateRight((ulong)length, 23) + ~seed;
            ulong d = ~(ulong)length + RotateRight(seed, 19);

            int limit = length - 32;
            while (offset <= limit)
            {
                ulong w0 = Read64(data, offset);
                ulong w1 = Read64(data, offset + 8);
                ulong w2 = Read64(data, offset + 16);
                ulong w3 = Read64(data, offset + 24);
                offset += 32;

                ulong d02 = w0 + RotateRight(w2 + d, 56);
                ulong c13 = w1 + RotateRight(w3 + c, 19);
                d ^= b + RotateRight(w1, 38);
                c ^= a + RotateRight(w0, 57);
                b ^= Prime6 * (c13 + w2);
                a ^= Prime5 * (d02 + w3);
            }

            a ^= Prime6 * (c + RotateRight(d, 23));
            b ^= Prime5 * (RotateRight(c, 19) + d);
        }

        int tailLength = length - offset;
        ReadOnlySpan<byte> tail = data.Slice(offset);

        if (tailLength > 24)
        {
            MixUp(ref a, ref b, Read64(tail, 0), Prime4);
            tail = tail.Slice(8);
            tailLength -= 8;
        }

        if (tailLength > 16)
        {
            MixUp(ref b, ref a, Read64(tail, 0), Prime3);
            tail = tail.Slice(8);
            tailLength -= 8;
        }

        if (tailLength > 8)
        {
            MixUp(ref a, ref b, Read64(tail, 0), Prime2);
            tail = tail.Slice(8);
            tailLength -= 8;
        }

        if (tailLength > 0)
            MixUp(ref b, ref a, Tail64(tail, tailLength), Prime1);

        return Final64(a, b);
    }
}