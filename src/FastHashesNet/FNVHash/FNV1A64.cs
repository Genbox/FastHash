//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_64a.c

namespace Genbox.FastHashesNet.FNVHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class FNV1A64
{
    public static ulong ComputeHash(byte[] data)
    {
        ulong hash = FNVConstants.FnvInit64;

        for (int i = 0; i < data.Length; i++)
        {
            hash ^= data[i];
            hash *= FNVConstants.FnvPrime64;
        }

        return hash;
    }
}