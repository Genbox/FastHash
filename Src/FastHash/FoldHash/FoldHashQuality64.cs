using System.Runtime.CompilerServices;
using static Genbox.FastHash.FoldHash.FoldHashConstants;

namespace Genbox.FastHash.FoldHash;

public static class FoldHashQuality64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndexCore(input, 0, DefaultSharedSeed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed) => ComputeIndexCore(input, seed, DefaultSharedSeed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ComputeIndexCore(ulong input, ulong seed, ulong[] sharedSeed)
    {
        ulong perHasherSeed = FoldHashShared.FoldedMultiply(seed, ARBITRARY4) ^ ARBITRARY3;
        ulong accumulator = FoldHashShared.RotateRight(perHasherSeed, 8);

        ulong s0 = accumulator ^ input;
        ulong s1 = sharedSeed[1] ^ input;
        ulong hash = FoldHashShared.FoldedMultiply(s0, s1);
        return FoldHashShared.FoldedMultiply(hash, ARBITRARY0);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0, ulong[]? sharedSeed = null)
    {
        sharedSeed ??= DefaultSharedSeed;

        ulong perHasherSeed = FoldHashShared.FoldedMultiply(seed, ARBITRARY4) ^ ARBITRARY3;
        ulong accumulator = FoldHashShared.RotateRight(perHasherSeed, data.Length);

        ulong hash = data.Length <= 16
            ? FoldHashShared.HashBytesShort(data, accumulator, sharedSeed)
            : FoldHashShared.HashBytesLong(data, accumulator, sharedSeed);

        return FoldHashShared.FoldedMultiply(hash, ARBITRARY0);
    }
}
