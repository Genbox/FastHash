//Ported to C# by Ian Qvist
//Source: http://www.cse.yorku.ca/~oz/hash.html

using System.Runtime.CompilerServices;
using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

public static class Djb2Hash32
{
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        uint hash = InitHash;

        for (int i = 0; i < data.Length; i++)
            hash = ((hash << 5) + hash) ^ data[i];

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = InitHash;
        hash = ((hash << 5) + hash) ^ (input & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 8) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 16) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 24) & 0xFF);
        return hash;
    }
}