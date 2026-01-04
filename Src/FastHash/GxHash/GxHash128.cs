#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash128
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input, long seed = 0)
    {
        Vector128<byte> hashVector = Vector128.Create(input, 0UL).AsByte();
        hashVector = Vector128.Add(hashVector, Vector128.Create((byte)8));
        Vector128<byte> hash = GxHashShared.Finalize(hashVector, seed);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> bytes, long seed = 0)
    {
        Vector128<byte> hash = GxHashShared.Finalize(GxHashShared.Compress(bytes), seed);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }
}
#endif