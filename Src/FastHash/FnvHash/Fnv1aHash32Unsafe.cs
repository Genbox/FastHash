using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Fowler–Noll–Vo hash implementation</summary>
public static class Fnv1aHash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        uint hash = FNV1_32_INIT;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[i];
            hash *= FNV_32_PRIME;
        }

        return hash;
    }
}