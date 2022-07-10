//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_32a.c

namespace Genbox.FastHashesNet.FNVHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class FNV1AYoshimitsuTRIAD32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        const uint PRIME = 709607;
        uint hash32 = 2166136261;
        uint hash32B = 2166136261;
        uint hash32C = 2166136261;
        //uint hash32D = 2166136261;
        byte* p = data;

        for (; length >= 3 * 2 * sizeof(uint); length -= 3 * 2 * sizeof(uint), p += 3 * 2 * sizeof(uint))
        {
            hash32 = (hash32 ^ (Utilities.Rotate(*(uint*)(p + 0), 5) ^ *(uint*)(p + 4))) * PRIME;
            hash32B = (hash32B ^ (Utilities.Rotate(*(uint*)(p + 8), 5) ^ *(uint*)(p + 12))) * PRIME;
            hash32C = (hash32C ^ (Utilities.Rotate(*(uint*)(p + 16), 5) ^ *(uint*)(p + 20))) * PRIME;

        }

        if (p != data)
        {
            hash32 = (hash32 ^ Utilities.Rotate(hash32C, 5)) * PRIME;
            //hash32B = (hash32B ^ Utilities.Rotate(hash32D,5) ) * PRIME;
        }

        // 1111=15; 10111=23
        if ((length & 4 * sizeof(uint)) > 0)
        {
            hash32 = (hash32 ^ (Utilities.Rotate(*(uint*)(p + 0), 5) ^ *(uint*)(p + 4))) * PRIME;
            hash32B = (hash32B ^ (Utilities.Rotate(*(uint*)(p + 8), 5) ^ *(uint*)(p + 12))) * PRIME;
            p += 8 * sizeof(ushort);
        }
        // Cases: 0,1,2,3,4,5,6,7,...,15
        if ((length & 2 * sizeof(uint)) > 0)
        {
            hash32 = (hash32 ^ *(uint*)(p + 0)) * PRIME;
            hash32B = (hash32B ^ *(uint*)(p + 4)) * PRIME;
            p += 4 * sizeof(ushort);
        }
        // Cases: 0,1,2,3,4,5,6,7
        if ((length & sizeof(uint)) > 0)
        {
            hash32 = (hash32 ^ *(ushort*)(p + 0)) * PRIME;
            hash32B = (hash32B ^ *(ushort*)(p + 2)) * PRIME;
            p += 2 * sizeof(ushort);
        }
        if ((length & sizeof(ushort)) > 0)
        {
            hash32 = (hash32 ^ *(ushort*)p) * PRIME;
            p += sizeof(ushort);
        }
        if ((length & 1) > 0)
            hash32 = (hash32 ^ *p) * PRIME;

        hash32 = (hash32 ^ Utilities.Rotate(hash32B, 5)) * PRIME;
        return hash32 ^ (hash32 >> 16);
    }
}