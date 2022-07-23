//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_64a.c

namespace Genbox.FastHash.FnvHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class Fnv1aHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        ulong hash = FnvHashConstants.FnvInit64;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[i];
            hash *= FnvHashConstants.FnvPrime64;
        }

        return hash;
    }
}