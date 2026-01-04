#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.MeowHash;

public static class MeowHash64Unsafe
{
    public static ulong ComputeIndex(ulong input)
    {
        Vector128<byte> res = MeowHash128Unsafe.ComputeIndexVector(input);
        return res.AsUInt64().GetElement(0);
    }

    public static unsafe ulong ComputeHash(byte* data, int len)
    {
        UInt128 res = MeowHash128Unsafe.ComputeHash(data, len);
        return res.Low;
    }
}
#endif