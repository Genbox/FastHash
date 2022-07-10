//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_32a.c

namespace FastHashesNet.FNVHash;

/// <summary>
/// Fowler–Noll–Vo hash implementation
/// </summary>
public static class FNV1AWHIZ32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        const uint PRIME = 1607;
        uint hash32 = 2166136261;

        byte* p = data;

        for (; length >= sizeof(uint); length -= sizeof(uint), p += sizeof(uint))
        {
            hash32 = (hash32 ^ *(uint*)p) * PRIME;
        }

        int val = length & sizeof(ushort);

        if (val > 0)
        {
            hash32 = (hash32 ^ *(ushort*)p) * PRIME;
            p += sizeof(ushort);
        }
        if ((length & 1) > 0)
            hash32 = (hash32 ^ *p) * PRIME;

        return hash32 ^ (hash32 >> 16);
    }
}