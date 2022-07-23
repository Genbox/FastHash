//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_64a.c

using System.Runtime.CompilerServices;

namespace Genbox.FastHash.FnvHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class Fnv1aHash64
{
    public static ulong ComputeHash(byte[] data)
    {
        ulong hash = FnvHashConstants.FnvInit64;

        for (int i = 0; i < data.Length; i++)
        {
            hash ^= data[i];
            hash *= FnvHashConstants.FnvPrime64;
        }

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong hash = FnvHashConstants.FnvInit64;
        hash ^= input;
        hash *= FnvHashConstants.FnvPrime64;
        return hash;
    }
}