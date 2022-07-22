//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_32a.c

using System.Runtime.CompilerServices;

namespace Genbox.FastHash.FNVHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class Fnv1aHash32
{
    public static uint ComputeHash(byte[] data)
    {
        uint hash = FnvHashConstants.FnvInit;

        for (int i = 0; i < data.Length; i++)
        {
            hash ^= data[i];
            hash *= FnvHashConstants.FnvPrime;
        }

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = FnvHashConstants.FnvInit;
        hash ^= input;
        hash *= FnvHashConstants.FnvPrime;
        return hash;
    }
}