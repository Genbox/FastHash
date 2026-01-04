using static Genbox.FastHash.FoldHash.FoldHashConstants;

namespace Genbox.FastHash.FoldHash;

public static class FoldHash64Quality
{
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
