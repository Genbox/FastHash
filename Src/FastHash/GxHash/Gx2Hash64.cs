#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class Gx2Hash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, long seed)
    {
        Vector128<byte> inputVector = Vector128.Create(input, 0UL).AsByte();
        Vector128<byte> lenVector = Vector128.Add(inputVector, Vector128.Create((byte)sizeof(ulong)));
        Vector128<byte> hash = Gx2HashShared.Compute(lenVector, seed);
        return Unsafe.As<Vector128<byte>, ulong>(ref hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> data, long seed)
    {
        Vector128<byte> hash = Gx2HashShared.Compute(data, seed);
        return Unsafe.As<Vector128<byte>, ulong>(ref hash);
    }
}
#endif