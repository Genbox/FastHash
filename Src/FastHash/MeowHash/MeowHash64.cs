#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.MeowHash;

public static class MeowHash64
{
    public static bool IsSupported => MeowHash128.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("MeowHash requires AES, SSE, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = MeowHash128.HashLen8(input);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        Vector128<byte> res = MeowHash128.ComputeHashVector(data);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }
}
#endif