#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash128V2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> bytes, UInt128 seed)
    {
        Vector128<byte> hash = GxHashV2Shared.CompressFast(GxHashV2Shared.Compress(bytes), Unsafe.As<UInt128, Vector128<byte>>(ref seed));
        hash = GxHashV2Shared.Finalize(hash);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }
}
#endif
