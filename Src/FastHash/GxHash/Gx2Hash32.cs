#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.GxHash;

public static class Gx2Hash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, long seed = 0)
    {
        Vector128<byte> inputVector = Vector128.Create(input, 0u, 0u, 0u).AsByte();
        Vector128<byte> lenVector = Vector128.Add(inputVector, Vector128.Create((byte)sizeof(uint)));
        Vector128<byte> hash = Gx2HashShared.Compute(lenVector, seed);
        return Unsafe.As<Vector128<byte>, uint>(ref hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeHash(ReadOnlySpan<byte> data, long seed = 0)
    {
        Vector128<byte> hash = Gx2HashShared.Compute(data, seed);
        return Unsafe.As<Vector128<byte>, uint>(ref hash);
    }
}
#endif