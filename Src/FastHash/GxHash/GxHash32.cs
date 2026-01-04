#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, long seed = 0)
    {
        Vector128<byte> hashVector = Vector128.Create(input, 0u, 0u, 0u).AsByte();
        hashVector = Vector128.Add(hashVector, Vector128.Create((byte)4));
        return GxHashShared.Finalize(hashVector, seed).AsUInt32().GetElement(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeHash(ReadOnlySpan<byte> bytes, long seed = 0) => GxHashShared.Finalize(GxHashShared.Compress(bytes), seed).AsUInt32().GetElement(0);
}
#endif