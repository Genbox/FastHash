#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.AesniHash;

public static class AesniHash64
{
    public static bool IsSupported => AesniHash128.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, uint seed)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("AesniHash requires AES, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = AesniHash128.Hash128Len8(input, seed);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, uint seed)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("AesniHash requires AES, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = AesniHash128.Hash128(data, seed);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }
}
#endif