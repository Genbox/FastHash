// C# port of orlp/foldhash 0.2.0 (8f878c6). Copyright (c) 2024 Orson Peters.
// Distributed under the zlib license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    internal static ulong FoldedMultiply(ulong x, ulong y) => Fold128To64(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong HashBytesShort(ReadOnlySpan<byte> bytes, ulong accumulator, ulong[] seeds)
    {
        ref byte bytesReference = ref MemoryMarshal.GetReference(bytes);
        int len = bytes.Length;
        ulong s0 = accumulator;
        ulong s1 = seeds[1];

        if (len >= 8)
        {
            s0 ^= Read64(ref bytesReference);
            s1 ^= Read64(ref bytesReference, len - 8);
        }
        else if (len >= 4)
        {
            s0 ^= Read32(ref bytesReference);
            s1 ^= Read32(ref bytesReference, len - 4);
        }
        else if (len > 0)
        {
            byte lo = Unsafe.Add(ref bytesReference, 0);
            byte mid = Unsafe.Add(ref bytesReference, len / 2);
            byte hi = Unsafe.Add(ref bytesReference, len - 1);
            s0 ^= lo;
            s1 ^= ((ulong)hi << 8) | mid;
        }

        return FoldedMultiply(s0, s1);
    }

    internal static ulong HashBytesLong(ReadOnlySpan<byte> data, ulong accumulator, ulong[] seeds)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        ulong s0 = accumulator;
        ulong seed0 = seeds[0];
        ulong s1 = s0 + seeds[1];
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
                    s0 = FoldedMultiply(Read64(ref dataReference) ^ s0, Read64(ref dataReference, 48) ^ seed0);
                    s1 = FoldedMultiply(Read64(ref dataReference, 8) ^ s1, Read64(ref dataReference, 56) ^ seed0);
                    s2 = FoldedMultiply(Read64(ref dataReference, 16) ^ s2, Read64(ref dataReference, 64) ^ seed0);
                    s3 = FoldedMultiply(Read64(ref dataReference, 24) ^ s3, Read64(ref dataReference, 72) ^ seed0);
                    s4 = FoldedMultiply(Read64(ref dataReference, 32) ^ s4, Read64(ref dataReference, 80) ^ seed0);
                    s5 = FoldedMultiply(Read64(ref dataReference, 40) ^ s5, Read64(ref dataReference, 88) ^ seed0);
                    dataReference = ref Unsafe.Add(ref dataReference, 96);
                    len -= 96;
                }

                s0 ^= s4;
                s1 ^= s5;
            }

            while (len > 128)
            {
                s0 = FoldedMultiply(Read64(ref dataReference) ^ s0, Read64(ref dataReference, 32) ^ seed0);
                s1 = FoldedMultiply(Read64(ref dataReference, 8) ^ s1, Read64(ref dataReference, 40) ^ seed0);
                s2 = FoldedMultiply(Read64(ref dataReference, 16) ^ s2, Read64(ref dataReference, 48) ^ seed0);
                s3 = FoldedMultiply(Read64(ref dataReference, 24) ^ s3, Read64(ref dataReference, 56) ^ seed0);
                dataReference = ref Unsafe.Add(ref dataReference, 64);
                len -= 64;
            }

            s0 ^= s2;
            s1 ^= s3;
        }

        s0 = FoldedMultiply(Read64(ref dataReference) ^ s0, Read64(ref dataReference, len - 16) ^ seed0);
        s1 = FoldedMultiply(Read64(ref dataReference, 8) ^ s1, Read64(ref dataReference, len - 8) ^ seed0);

        if (len >= 32)
        {
            s0 = FoldedMultiply(Read64(ref dataReference, 16) ^ s0, Read64(ref dataReference, len - 32) ^ seed0);
            s1 = FoldedMultiply(Read64(ref dataReference, 24) ^ s1, Read64(ref dataReference, len - 24) ^ seed0);

            if (len >= 64)
            {
                s0 = FoldedMultiply(Read64(ref dataReference, 32) ^ s0, Read64(ref dataReference, len - 48) ^ seed0);
                s1 = FoldedMultiply(Read64(ref dataReference, 40) ^ s1, Read64(ref dataReference, len - 40) ^ seed0);

                if (len >= 96)
                {
                    s0 = FoldedMultiply(Read64(ref dataReference, 48) ^ s0, Read64(ref dataReference, len - 64) ^ seed0);
                    s1 = FoldedMultiply(Read64(ref dataReference, 56) ^ s1, Read64(ref dataReference, len - 56) ^ seed0);
                }
            }
        }

        return s0 ^ s1;
    }
}