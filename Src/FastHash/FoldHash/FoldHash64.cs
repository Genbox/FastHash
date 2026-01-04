using static Genbox.FastHash.FoldHash.FoldHashConstants;

namespace Genbox.FastHash.FoldHash;

public static class FoldHash64
{
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0, ulong[]? sharedSeed = null)
    {
        sharedSeed ??= DefaultSharedSeed;

        ulong perHasherSeed = seed ^ ARBITRARY3;
        ulong accumulator = FoldHashShared.RotateRight(perHasherSeed, data.Length);

        if (data.Length <= 16)
            return FoldHashShared.HashBytesShort(data, accumulator, sharedSeed);

        return FoldHashShared.HashBytesLong(data, accumulator, sharedSeed);
    }
}
