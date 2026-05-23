#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.MeowHash;

public static class MeowHash64Unsafe
{
    public static bool IsSupported => MeowHash128Unsafe.IsSupported;

    public static ulong ComputeIndex(ulong input)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("MeowHash requires AES, SSE, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = MeowHash128Unsafe.ComputeIndexVector(input);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }

    public static unsafe ulong ComputeHash(byte* data, int len)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("MeowHash requires AES, SSE, SSE2, and SSSE3 intrinsics.");

        UInt128 res = MeowHash128Unsafe.ComputeHash(data, len);
        return res.Low;
    }
}
#endif