#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class GxHash64V2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> bytes, UInt128 seed)
    {
        Vector128<byte> hash = GxHashV2Shared.CompressFast(GxHashV2Shared.Compress(bytes), Unsafe.As<UInt128, Vector128<byte>>(ref seed));
        return GxHashV2Shared.Finalize(hash).AsUInt64().GetElement(0);
    }
}
#endif
