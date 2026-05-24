namespace Genbox.FastHash.CityHash;

public static class CityHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length) => CityHash64.ComputeHash(new ReadOnlySpan<byte>(data, length));

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed) => CityHash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1, ulong seed2) => CityHash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed1, seed2);
}