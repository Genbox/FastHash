namespace Genbox.FastHash.Benchmarks.Code;

public static class BenchmarkHelper
{
    public static byte[] GetRandomBytes(int count)
    {
        byte[] bytes = GC.AllocateUninitializedArray<byte>(count);
        Random r = new Random(42);
        r.NextBytes(bytes);
        return bytes;
    }
}