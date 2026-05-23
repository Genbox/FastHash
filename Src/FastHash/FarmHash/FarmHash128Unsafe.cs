using Genbox.FastHash.CityHash;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash128Unsafe
{
    // farmhashcc's 128-bit path is CityHash128 v1.1.1.

    public static unsafe UInt128 ComputeHash(byte* data, int length) => CityHash128Unsafe.ComputeHash(data, length);

    public static unsafe UInt128 ComputeHash(byte* data, int length, UInt128 seed) => CityHash128Unsafe.ComputeHash(data, length, seed);
}