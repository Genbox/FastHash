using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

public static class CityHashCrc128Unsafe
{
    public static unsafe Uint128 ComputeHash(byte* s, int length)
    {
        if (length <= 900)
            return CityHash128Unsafe.ComputeHash(s, length);

        ulong[] result = new ulong[4];
        CityHashCrc256Unsafe.ComputeHash(s, length, result);
        return new Uint128(result[2], result[3]);
    }

    public static unsafe Uint128 ComputeHash(byte* s, int length, Uint128 seed)
    {
        uint len = (uint)length;

        if (len <= 900)
            return CityHash128Unsafe.CityHash128WithSeed(s, len, seed);

        ulong[] result = new ulong[4];
        CityHashCrc256Unsafe.ComputeHash(s, length, result);
        ulong u = seed.High + result[0];
        ulong v = seed.Low + result[1];
        return new Uint128(HashLen16(u, v + result[2]), HashLen16(RotateRight(v, 32), u * K0 + result[3]));
    }
}