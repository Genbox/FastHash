//Ported to C# by Ian Qvist
//Source: http://www.cse.yorku.ca/~oz/hash.html

using System.Runtime.CompilerServices;

namespace Genbox.FastHash.DjbHash;

public static class Djb2Hash32
{
    public static uint ComputeHash(byte[] data)
    {
        uint hash = DjbHashConstants.InitHash;

        for (int x = 0; x < data.Length; x++)
            hash = ((hash << 5) + hash) ^ data[x];

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = DjbHashConstants.InitHash;
        hash = ((hash << 5) + hash) ^ input & 0xFF;
        hash = ((hash << 5) + hash) ^ (input >> 8) & 0xFF;
        hash = ((hash << 5) + hash) ^ (input >> 16) & 0xFF;
        hash = ((hash << 5) + hash) ^ (input >> 24) & 0xFF;
        return hash;
    }
}