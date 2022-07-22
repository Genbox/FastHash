//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_32a.c

namespace Genbox.FastHash.FNVHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class FNV1a32
{
    public static uint ComputeHash(byte[] data)
    {
        uint hash = FNVConstants.FnvInit;

        for (int i = 0; i < data.Length; i++)
        {
            hash ^= data[i];
            hash *= FNVConstants.FnvPrime;
        }

        return hash;
    }
}