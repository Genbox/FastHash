using System.Runtime.CompilerServices;
using Genbox.FastHash.CityHash;

namespace Genbox.FastHash.FarmHash;

public static class FarmHash128
{
    // farmhashcc's 128-bit path is CityHash128 v1.1.1.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input) => CityHash128.ComputeIndex(input);

    public static UInt128 ComputeHash(ReadOnlySpan<byte> data) => CityHash128.ComputeHash(data);

    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, UInt128 seed) => CityHash128.ComputeHash(data, seed);
}