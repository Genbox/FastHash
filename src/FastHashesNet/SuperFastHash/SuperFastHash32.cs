//Ported to C# by Ian Qvist
//Source: http://www.azillionmonkeys.com/qed/hash.html

//Note: This algorithm has known issues. https://floodyberry.wordpress.com/2007/03/29/breaking-superfasthash/

namespace Genbox.FastHashesNet.SuperFastHash;

public static class SuperFastHash32
{
    public static uint ComputeHash(byte[] data)
    {
        if (data == null || data.Length <= 0)
            return 0;

        int length = data.Length;
        uint hash = (uint)length, tmp;
        int rem = length & 3;
        length >>= 2;

        uint index = 0;

        for (; length > 0; length--)
        {
            hash += Utilities.Fetch16(data, index);
            tmp = (uint)((Utilities.Fetch16(data, index + 2) << 11) ^ hash);
            hash = (hash << 16) ^ tmp;
            index += 2 * sizeof(ushort);
            hash += hash >> 11;
        }

        switch (rem)
        {
            case 3:
                hash += Utilities.Fetch16(data, index);
                hash ^= hash << 16;
                hash ^= (uint)(data[index + sizeof(ushort)] << 18);
                hash += hash >> 11;
                break;
            case 2:
                hash += Utilities.Fetch16(data, index);
                hash ^= hash << 11;
                hash += hash >> 17;
                break;
            case 1:
                hash += data[index];
                hash ^= hash << 10;
                hash += hash >> 1;
                break;
        }

        // Force "avalanching" of final 127 bits
        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        hash += hash >> 6;

        return hash;
    }
}