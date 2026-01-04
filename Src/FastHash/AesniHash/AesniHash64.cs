#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.AesniHash;

public static class AesniHash64
{
    public static ulong ComputeHash(ReadOnlySpan<byte> data, uint seed = 0)
    {
        Vector128<byte> res = AesniHash128.Hash128(data, seed);
        return Unsafe.As<Vector128<byte>, ulong>(ref res);
    }
}
#endif