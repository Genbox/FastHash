#if NET8_0
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.AesniHash;

public static class AesniHash64
{
    public static uint ComputeIndex(uint input) => 0;

    public static ulong ComputeHash(ReadOnlySpan<byte> data, uint seed = 0)
    {
        Vector128<ulong> res = AesniHash128.Hash128(data, seed).AsUInt64();
        return res[0];
    }
}
#endif