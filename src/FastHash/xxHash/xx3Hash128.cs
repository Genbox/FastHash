namespace Genbox.FastHash.xxHash;

public static class xx3Hash128
{
    public static Uint128 ComputeHash(byte[] input)
    {
        unsafe
        {
            fixed (byte* ptr = input)
                return xx3Hash128Unsafe.ComputeHash(ptr, input.Length);
        }
    }
}