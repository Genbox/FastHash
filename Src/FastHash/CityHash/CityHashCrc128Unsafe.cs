#if NET8_0_OR_GREATER
using static Genbox.FastHash.CityHash.CityHashConstants;
using static Genbox.FastHash.CityHash.CityHashShared;

namespace Genbox.FastHash.CityHash;

public static class CityHashCrc128Unsafe
{
    public static bool IsSupported => CityHashCrc256Unsafe.IsSupported;

    public static unsafe UInt128 ComputeHash(byte* data, int length)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        return ComputeHashInternal(data, (uint)length);
    }

    public static unsafe UInt128 ComputeHash(byte* data, int length, UInt128 seed)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        return ComputeHashInternal(data, (uint)length, seed);
    }

    internal static unsafe UInt128 ComputeHashInternal(byte* data, uint length)
    {
        if (length <= 900)
            return CityHash128Unsafe.ComputeHash(data, (int)length);

        ulong* result = stackalloc ulong[4];
        CityHashCrc256Unsafe.ComputeHashInternal(data, length, result);
        return new UInt128(result[2], result[3]);
    }

    internal static unsafe UInt128 ComputeHashInternal(byte* data, uint length, UInt128 seed)
    {
        if (length <= 900)
            return CityHash128Unsafe.ComputeHash(data, (int)length, seed);

        ulong* result = stackalloc ulong[4];
        CityHashCrc256Unsafe.ComputeHashInternal(data, length, result);
        ulong u = seed.High + result[0];
        ulong v = seed.Low + result[1];
        return new UInt128(HashLen16(u, v + result[2]), HashLen16(RotateRight(v, 32), (u * K0) + result[3]));
    }
}
#endif