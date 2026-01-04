using System.Runtime.CompilerServices;

namespace Genbox.FastHash.FoldHash;

internal static class FoldHashShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong FoldedMultiply(ulong x, ulong y)
    {
        ulong high = BigMul(x, y, out ulong low);
        return low ^ high;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RotateRight(ulong value, int count)
    {
        int r = count & 63;
        return r == 0 ? value : (value >> r) | (value << (64 - r));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong HashBytesShort(ReadOnlySpan<byte> bytes, ulong accumulator, ulong[] seeds)
    {
        int len = bytes.Length;
        ulong s0 = accumulator;
        ulong s1 = seeds[1];

        if (len >= 8)
        {
            s0 ^= Read64(bytes);
            s1 ^= Read64(bytes, len - 8);
        }
        else if (len >= 4)
        {
            s0 ^= Read32(bytes);
            s1 ^= Read32(bytes, len - 4);
        }
        else if (len > 0)
        {
            byte lo = bytes[0];
            byte mid = bytes[len / 2];
            byte hi = bytes[len - 1];
            s0 ^= lo;
            s1 ^= ((ulong)hi << 8) | mid;
        }

        return FoldedMultiply(s0, s1);
    }

    internal static ulong HashBytesLong(ReadOnlySpan<byte> data, ulong accumulator, ulong[] seeds)
    {
        ulong s0 = accumulator;
        ulong s1 = s0 + seeds[1];

        if (data.Length > 128)
        {
            ulong s2 = s0 + seeds[2];
            ulong s3 = s0 + seeds[3];

            if (data.Length > 256)
            {
                ulong s4 = s0 + seeds[4];
                ulong s5 = s0 + seeds[5];

                while (data.Length > 256)
                {
                    s0 = FoldedMultiply(Read64(data) ^ s0, Read64(data, 48) ^ seeds[0]);
                    s1 = FoldedMultiply(Read64(data, 8) ^ s1, Read64(data, 56) ^ seeds[0]);
                    s2 = FoldedMultiply(Read64(data, 16) ^ s2, Read64(data, 64) ^ seeds[0]);
                    s3 = FoldedMultiply(Read64(data, 24) ^ s3, Read64(data, 72) ^ seeds[0]);
                    s4 = FoldedMultiply(Read64(data, 32) ^ s4, Read64(data, 80) ^ seeds[0]);
                    s5 = FoldedMultiply(Read64(data, 40) ^ s5, Read64(data, 88) ^ seeds[0]);
                    data = data.Slice(96);
                }

                s0 ^= s4;
                s1 ^= s5;
            }

            while (data.Length > 128)
            {
                s0 = FoldedMultiply(Read64(data) ^ s0, Read64(data, 32) ^ seeds[0]);
                s1 = FoldedMultiply(Read64(data, 8) ^ s1, Read64(data, 40) ^ seeds[0]);
                s2 = FoldedMultiply(Read64(data, 16) ^ s2, Read64(data, 48) ^ seeds[0]);
                s3 = FoldedMultiply(Read64(data, 24) ^ s3, Read64(data, 56) ^ seeds[0]);
                data = data.Slice(64);
            }

            s0 ^= s2;
            s1 ^= s3;
        }

        int len = data.Length;

        s0 = FoldedMultiply(Read64(data) ^ s0, Read64(data, len - 16) ^ seeds[0]);
        s1 = FoldedMultiply(Read64(data, 8) ^ s1, Read64(data, len - 8) ^ seeds[0]);

        if (len >= 32)
        {
            s0 = FoldedMultiply(Read64(data, 16) ^ s0, Read64(data, len - 32) ^ seeds[0]);
            s1 = FoldedMultiply(Read64(data, 24) ^ s1, Read64(data, len - 24) ^ seeds[0]);

            if (len >= 64)
            {
                s0 = FoldedMultiply(Read64(data, 32) ^ s0, Read64(data, len - 48) ^ seeds[0]);
                s1 = FoldedMultiply(Read64(data, 40) ^ s1, Read64(data, len - 40) ^ seeds[0]);

                if (len >= 96)
                {
                    s0 = FoldedMultiply(Read64(data, 48) ^ s0, Read64(data, len - 64) ^ seeds[0]);
                    s1 = FoldedMultiply(Read64(data, 56) ^ s1, Read64(data, len - 56) ^ seeds[0]);
                }
            }
        }

        return s0 ^ s1;
    }
}
