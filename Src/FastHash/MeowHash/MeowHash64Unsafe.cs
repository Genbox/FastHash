﻿#if NET8_0
namespace Genbox.FastHash.MeowHash;

public static class MeowHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int len)
    {
        UInt128 res = MeowHash128Unsafe.ComputeHash(data, len);
        return res.Low;
    }
}
#endif