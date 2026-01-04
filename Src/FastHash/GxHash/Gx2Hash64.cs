#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class Gx2Hash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, UInt128 seed)
    {
        Vector128<byte> hashVector = Vector128.Create(input, 0UL).AsByte();
        hashVector = Vector128.Add(hashVector, Vector128.Create((byte)8));
        Vector128<byte> seedVector = Unsafe.As<UInt128, Vector128<byte>>(ref seed);
        Vector128<byte> hash = Gx2HashShared.CompressFast(hashVector, seedVector);
        return Gx2HashShared.Finalize(hash).AsUInt64().GetElement(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> bytes, UInt128 seed)
    {
        Vector128<byte> hash = Gx2HashShared.CompressFast(Gx2HashShared.Compress(bytes), Unsafe.As<UInt128, Vector128<byte>>(ref seed));
        return Gx2HashShared.Finalize(hash).AsUInt64().GetElement(0);
    }
}
#endif