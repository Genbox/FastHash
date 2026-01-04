#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, long seed = 0)
    {
        Vector128<byte> hashVector = Vector128.Create(input, 0UL).AsByte();
        hashVector = Vector128.Add(hashVector, Vector128.Create((byte)8));
        return GxHashShared.Finalize(hashVector, seed).AsUInt64().GetElement(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> bytes, long seed = 0) => GxHashShared.Finalize(GxHashShared.Compress(bytes), seed).AsUInt64().GetElement(0);
}
#endif