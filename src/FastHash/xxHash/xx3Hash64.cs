namespace Genbox.FastHash.xxHash;

public static class xx3Hash64
{
    public static ulong ComputeHash(byte[] input)
    {
        unsafe
        {
            fixed (byte* ptr = input)
                return xx3Hash64Unsafe.ComputeHash(ptr, input.Length);
        }
    }
}