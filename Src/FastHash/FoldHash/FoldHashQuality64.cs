using System.Runtime.CompilerServices;
using static Genbox.FastHash.FoldHash.FoldHashConstants;

namespace Genbox.FastHash.FoldHash;

public static class FoldHashQuality64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong hash = FoldHashShared.FoldedMultiply(0x89082efa98ec4e6cUL ^ input, ARBITRARY7 ^ input);
        return FoldHashShared.FoldedMultiply(hash, ARBITRARY0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed) => ComputeIndexCore(input, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ComputeIndexCore(ulong input, ulong seed)
    {
        ulong accumulator = FoldHashShared.RotateRight(FoldHashShared.FoldedMultiply(seed, ARBITRARY4) ^ ARBITRARY3, sizeof(ulong));
        ulong s0 = accumulator ^ input;
        ulong s1 = ARBITRARY7 ^ input;
        ulong hash = FoldHashShared.FoldedMultiply(s0, s1);
        return FoldHashShared.FoldedMultiply(hash, ARBITRARY0);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0, null);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => ComputeHash(data, seed, null);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[]? sharedSeed) => ComputeHash(data, 0, sharedSeed);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed, ulong[]? sharedSeed)
    {
        sharedSeed ??= DefaultSharedSeed;
        FoldHashShared.ValidateSharedSeed(sharedSeed, nameof(sharedSeed));

        ulong perHasherSeed = FoldHashShared.FoldedMultiply(seed, ARBITRARY4) ^ ARBITRARY3;
        ulong accumulator = FoldHashShared.RotateRight(perHasherSeed, data.Length);

        ulong hash = data.Length <= 16
            ? FoldHashShared.HashBytesShort(data, accumulator, sharedSeed)
            : FoldHashShared.HashBytesLong(data, accumulator, sharedSeed);

        return FoldHashShared.FoldedMultiply(hash, ARBITRARY0);
    }
}