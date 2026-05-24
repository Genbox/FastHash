namespace Genbox.FastHash.XxHash;

public static class XxHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length) => ComputeHash(data, length, 0);

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed) => XxHash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
}