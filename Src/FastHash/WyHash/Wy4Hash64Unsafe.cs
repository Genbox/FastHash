namespace Genbox.FastHash.WyHash;

public static class Wy4Hash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed = 0) => Wy4Hash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
}