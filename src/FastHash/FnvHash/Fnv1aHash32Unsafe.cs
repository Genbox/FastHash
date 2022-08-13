//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_32a.c

using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Fowler–Noll–Vo hash implementation</summary>
public static class Fnv1aHash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        uint hash = FnvInit;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[i];
            hash *= FnvPrime;
        }

        return hash;
    }
}