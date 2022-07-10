//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_64a.c

namespace FastHashesNet.FNVHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class FNV1A64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        ulong hash = FNVConstants.FnvInit64;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[i];
            hash *= FNVConstants.FnvPrime64;
        }

        return hash;
    }
}