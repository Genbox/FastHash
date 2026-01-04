#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class Gx2Hash128
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input, UInt128 seed)
    {
        Vector128<byte> hashVector = Vector128.Create(input, 0UL).AsByte();
        hashVector = Vector128.Add(hashVector, Vector128.Create((byte)8));
        Vector128<byte> seedVector = Unsafe.As<UInt128, Vector128<byte>>(ref seed);
        Vector128<byte> hash = Gx2HashShared.CompressFast(hashVector, seedVector);
        hash = Gx2HashShared.Finalize(hash);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> bytes, UInt128 seed)
    {
        Vector128<byte> hash = Gx2HashShared.CompressFast(Gx2HashShared.Compress(bytes), Unsafe.As<UInt128, Vector128<byte>>(ref seed));
        hash = Gx2HashShared.Finalize(hash);
        return Unsafe.As<Vector128<byte>, UInt128>(ref hash);
    }
}
#endif