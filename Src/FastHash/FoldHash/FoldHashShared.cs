using System.Runtime.CompilerServices;

namespace Genbox.FastHash.FoldHash;

internal static class FoldHashShared
{
    internal const int SharedSeedLength = 6;

    internal static void ValidateSharedSeed(ulong[] seeds, string paramName)
    {
        if (seeds.Length < SharedSeedLength)
            throw new ArgumentException($"Shared seed must contain at least {SharedSeedLength} values.", paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong FoldedMultiply(ulong x, ulong y) => Shared.Fold128To64(x, y);

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
        ulong seed0 = seeds[0];
        ulong s1 = s0 + seeds[1];
        int offset = 0;
        int len = data.Length;

        if (len > 128)
        {
            ulong s2 = s0 + seeds[2];
            ulong s3 = s0 + seeds[3];

            if (len > 256)
            {
                ulong s4 = s0 + seeds[4];
                ulong s5 = s0 + seeds[5];

                while (len > 256)
                {
                    s0 = FoldedMultiply(Read64(data, offset) ^ s0, Read64(data, offset + 48) ^ seed0);
                    s1 = FoldedMultiply(Read64(data, offset + 8) ^ s1, Read64(data, offset + 56) ^ seed0);
                    s2 = FoldedMultiply(Read64(data, offset + 16) ^ s2, Read64(data, offset + 64) ^ seed0);
                    s3 = FoldedMultiply(Read64(data, offset + 24) ^ s3, Read64(data, offset + 72) ^ seed0);
                    s4 = FoldedMultiply(Read64(data, offset + 32) ^ s4, Read64(data, offset + 80) ^ seed0);
                    s5 = FoldedMultiply(Read64(data, offset + 40) ^ s5, Read64(data, offset + 88) ^ seed0);
                    offset += 96;
                    len -= 96;
                }

                s0 ^= s4;
                s1 ^= s5;
            }

            while (len > 128)
            {
                s0 = FoldedMultiply(Read64(data, offset) ^ s0, Read64(data, offset + 32) ^ seed0);
                s1 = FoldedMultiply(Read64(data, offset + 8) ^ s1, Read64(data, offset + 40) ^ seed0);
                s2 = FoldedMultiply(Read64(data, offset + 16) ^ s2, Read64(data, offset + 48) ^ seed0);
                s3 = FoldedMultiply(Read64(data, offset + 24) ^ s3, Read64(data, offset + 56) ^ seed0);
                offset += 64;
                len -= 64;
            }

            s0 ^= s2;
            s1 ^= s3;
        }

        s0 = FoldedMultiply(Read64(data, offset) ^ s0, Read64(data, (offset + len) - 16) ^ seed0);
        s1 = FoldedMultiply(Read64(data, offset + 8) ^ s1, Read64(data, (offset + len) - 8) ^ seed0);

        if (len >= 32)
        {
            s0 = FoldedMultiply(Read64(data, offset + 16) ^ s0, Read64(data, (offset + len) - 32) ^ seed0);
            s1 = FoldedMultiply(Read64(data, offset + 24) ^ s1, Read64(data, (offset + len) - 24) ^ seed0);

            if (len >= 64)
            {
                s0 = FoldedMultiply(Read64(data, offset + 32) ^ s0, Read64(data, (offset + len) - 48) ^ seed0);
                s1 = FoldedMultiply(Read64(data, offset + 40) ^ s1, Read64(data, (offset + len) - 40) ^ seed0);

                if (len >= 96)
                {
                    s0 = FoldedMultiply(Read64(data, offset + 48) ^ s0, Read64(data, (offset + len) - 64) ^ seed0);
                    s1 = FoldedMultiply(Read64(data, offset + 56) ^ s1, Read64(data, (offset + len) - 56) ^ seed0);
                }
            }
        }

        return s0 ^ s1;
    }
}