//Ported to C# by Ian Qvist
//Source: http://www.azillionmonkeys.com/qed/hash.html

using System.Runtime.CompilerServices;

namespace Genbox.FastHash.SuperFastHash;

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
            hash += Read16(data, index);
            tmp = (uint)((Read16(data, index + 2) << 11) ^ hash);
            hash = (hash << 16) ^ tmp;
            index += 2 * sizeof(ushort);
            hash += hash >> 11;
        }

        switch (rem)
        {
            case 3:
                hash += Read16(data, index);
                hash ^= hash << 16;
                hash ^= (uint)(data[index + sizeof(ushort)] << 18);
                hash += hash >> 11;
                break;
            case 2:
                hash += Read16(data, index);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = 4 + (uint)(ushort)input;
        hash = (hash << 16) ^ ((input >> 16) << 11) ^ hash;
        hash += hash >> 11;
        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        hash += hash >> 6;
        return hash;
    }
}