using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

public static class Djb2Hash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        uint hash = InitHash;

        for (int i = 0; i < length; i++)
            hash = ((hash << 5) + hash) ^ data[i];

        return hash;
    }
}