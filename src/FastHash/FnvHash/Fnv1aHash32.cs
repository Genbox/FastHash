//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_32a.c

using System.Runtime.CompilerServices;
using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Fowler–Noll–Vo hash implementation</summary>
public static class Fnv1aHash32
{
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        uint hash = FnvInit;

        for (int i = 0; i < data.Length; i++)
            hash = (hash ^ data[i]) * FnvPrime;

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = FnvInit;
        hash = (hash ^ (input & 0xFF)) * FnvPrime;
        hash = (hash ^ ((input >> 8) & 0xFF)) * FnvPrime;
        hash = (hash ^ ((input >> 16) & 0xFF)) * FnvPrime;
        hash = (hash ^ ((input >> 24) & 0xFF)) * FnvPrime;
        return hash;
    }
}