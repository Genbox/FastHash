using static Genbox.FastHash.AbslHash.AbslHashShared;

namespace Genbox.FastHash.AbslHash;

public static class AbslHash64
{
    public static bool IsSimdSupported => AbslHashShared.IsSimdSupported;

    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => CombineContiguous(seed, data);
}