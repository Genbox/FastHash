using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Fowler–Noll–Vo hash implementation</summary>
public static class Fnv1aHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        ulong hash = FNV1_64_INIT;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[i];
            hash *= FNV_64_PRIME;
        }

        return hash;
    }
}