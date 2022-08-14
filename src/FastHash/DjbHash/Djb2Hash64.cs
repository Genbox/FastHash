using System.Runtime.CompilerServices;
using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

public static class Djb2Hash64
{
    public static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        ulong hash = InitHash;

        for (int i = 0; i < data.Length; i++)
            hash = ((hash << 5) + hash) ^ data[i];

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong hash = InitHash;
        hash = ((hash << 5) + hash) ^ (input & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 8) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 16) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 24) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 32) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 40) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 48) & 0xFF);
        hash = ((hash << 5) + hash) ^ ((input >> 56) & 0xFF);
        return hash;
    }
}