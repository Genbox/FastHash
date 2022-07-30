//Ported to C# by Ian Qvist
//Source: http://www.cse.yorku.ca/~oz/hash.html

using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

public static class Djb2Hash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        uint hash = InitHash;

        for (int x = 0; x < length; x++)
            hash = ((hash << 5) + hash) ^ data[x];

        return hash;
    }
}