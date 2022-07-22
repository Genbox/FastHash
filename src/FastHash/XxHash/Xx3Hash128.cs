namespace Genbox.FastHash.xxHash;

public static class Xx3Hash128
{
    public static Uint128 ComputeHash(byte[] input)
    {
        unsafe
        {
            fixed (byte* ptr = input)
                return Xx3Hash128Unsafe.ComputeHash(ptr, input.Length);
        }
    }
}