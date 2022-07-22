namespace Genbox.FastHash.xxHash;

public static class Xx3Hash64
{
    public static ulong ComputeHash(byte[] input)
    {
        unsafe
        {
            fixed (byte* ptr = input)
                return Xx3Hash64Unsafe.ComputeHash(ptr, input.Length);
        }
    }
}