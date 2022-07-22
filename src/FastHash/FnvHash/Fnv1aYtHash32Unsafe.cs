//Ported to C# by Ian Qvist
//Source: https://github.com/k0dai/fsbench-density/blob/cb719fdb07354e0c39a255d8d12f3cbd50b44406/src/codecs/sanmayce.c
//A FNV1a variant created by Georgi (Sanmayce)

namespace Genbox.FastHash.FNVHash;

/// <summary>
/// FNV1a-YoshimitsuTRIAD 32bit version
/// </summary>
public static class Fnv1aYtHash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        const uint PRIME = 709607;
        uint hash32 = 2166136261;
        uint hash32B = 2166136261;
        uint hash32C = 2166136261;
        byte* p = data;
        uint* uPtr = (uint*)data;

        for (; length >= 3 * sizeof(ulong); length -= 3 * sizeof(ulong), p += 3 * sizeof(ulong))
        {
            hash32 = (hash32 ^ Utilities.RotateLeft(*uPtr + 0, 5) ^ (*uPtr + 4)) * PRIME;
            hash32B = (hash32B ^ Utilities.RotateLeft(*uPtr + 8, 5) ^ (*uPtr + 12)) * PRIME;
            hash32C = (hash32C ^ Utilities.RotateLeft(*uPtr + 16, 5) ^ (*uPtr + 20)) * PRIME;
        }

        if (p != data)
            hash32 = (hash32 ^ Utilities.RotateLeft(hash32C, 5)) * PRIME;

        if ((length & (2 * sizeof(ulong))) > 0)
        {
            hash32 = (hash32 ^ Utilities.RotateLeft(*uPtr + 0, 5) ^ (*uPtr + 4)) * PRIME;
            hash32B = (hash32B ^ Utilities.RotateLeft(*uPtr + 8, 5) ^ (*uPtr + 12)) * PRIME;
            p += 2 * sizeof(ulong);
        }

        // Cases: 0,1,2,3,4,5,6,7,...,15
        if ((length & sizeof(ulong)) > 0)
        {
            hash32 = (hash32 ^ (*uPtr + 0)) * PRIME;
            hash32B = (hash32B ^ (*uPtr + 4)) * PRIME;
            p += sizeof(ulong);
        }

        // Cases: 0,1,2,3,4,5,6,7
        if ((length & sizeof(uint)) > 0)
        {
            hash32 = (hash32 ^ *(ushort*)(p + 0)) * PRIME;
            hash32B = (hash32B ^ *(ushort*)(p + 2)) * PRIME;
            p += sizeof(uint);
        }

        if ((length & sizeof(ushort)) > 0)
        {
            hash32 = (hash32 ^ *(ushort*)p) * PRIME;
            p += sizeof(ushort);
        }

        if ((length & 1) > 0)
            hash32 = (hash32 ^ *p) * PRIME;

        hash32 = (hash32 ^ Utilities.RotateLeft(hash32B, 5)) * PRIME;
        return hash32 ^ (hash32 >> 16);
    }
}