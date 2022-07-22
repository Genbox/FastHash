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
            hash = (hash ^ data[i]) * FnvHashConstants.FnvPrime;

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = FnvHashConstants.FnvInit;
        hash = (hash ^ input & 0xFF) * FnvHashConstants.FnvPrime;
        hash = (hash ^ (input >> 8) & 0xFF) * FnvHashConstants.FnvPrime;
        hash = (hash ^ (input >> 16) & 0xFF) * FnvHashConstants.FnvPrime;
        hash = (hash ^ (input >> 24) & 0xFF) * FnvHashConstants.FnvPrime;
        return hash;
    }
}