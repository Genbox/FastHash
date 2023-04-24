using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

public static class Djb2Hash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        ulong hash = InitHash;

        for (int i = 0; i < length; i++)
            hash = ((hash << 5) + hash) ^ data[i];

        return hash;
    }
}