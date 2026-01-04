#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeHash(ReadOnlySpan<byte> bytes, long seed = 0) => GxHashShared.Finalize(GxHashShared.Compress(bytes), seed).AsUInt32().GetElement(0);
}
#endif