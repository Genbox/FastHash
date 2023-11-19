#if NET8_0
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash128
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> bytes, long seed = 0)
    {
        Vector128<byte> hash = GxHashShared.Finalize(GxHashShared.Compress(bytes), seed);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }
}
#endif