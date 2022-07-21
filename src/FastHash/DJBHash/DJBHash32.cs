//Ported to C# by Ian Qvist
//Source: http://www.cse.yorku.ca/~oz/hash.html

namespace Genbox.FastHash.DJBHash;

public static class DJBHash32
{
    public static uint ComputeHash(byte[] data)
    {
        uint hash = DJBHashConstants.InitHash;

        for (int x = 0; x < data.Length; x++)
            hash = ((hash << 5) + hash) ^ data[x];

        return hash;
    }
}