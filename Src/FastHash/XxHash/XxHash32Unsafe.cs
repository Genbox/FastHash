namespace Genbox.FastHash.XxHash;

public static class XxHash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length) => ComputeHash(data, length, 0);

    public static unsafe uint ComputeHash(byte* data, int length, uint seed) => XxHash32.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
}