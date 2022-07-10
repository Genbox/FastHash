//Ported to C# by Ian Qvist
//Source: http://www.cse.yorku.ca/~oz/hash.html

namespace Genbox.FastHashesNet.DJBHash;

public static class DJBHash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        uint hash = DJBHashConstants.InitHash;

        for (int x = 0; x < length; x++)
            hash = ((hash << 5) + hash) ^ data[x];

        return hash;
    }
}