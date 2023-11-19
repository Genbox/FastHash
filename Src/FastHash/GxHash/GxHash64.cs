#if NET8_0
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> bytes, long seed = 0) => GxHashShared.Finalize(GxHashShared.Compress(bytes), seed).AsUInt64().GetElement(0);
}
#endif