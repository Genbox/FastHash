#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class Gx2Hash128
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Genbox.FastHash.UInt128 ComputeHash(ReadOnlySpan<byte> bytes, Genbox.FastHash.UInt128 seed)
    {
        Vector128<byte> hash = Gx2HashShared.CompressFast(Gx2HashShared.Compress(bytes), Unsafe.As<Genbox.FastHash.UInt128, Vector128<byte>>(ref seed));
        hash = Gx2HashShared.Finalize(hash);
        return Unsafe.As<Vector128<byte>, Genbox.FastHash.UInt128>(ref hash);
    }
}
#endif
