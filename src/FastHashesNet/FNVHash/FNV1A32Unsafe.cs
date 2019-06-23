//Ported to C# by Ian Qvist
//Source: http://www.isthe.com/chongo/src/fnv/hash_32a.c

namespace FastHashesNet.FNVHash
{
    /// <summary>
    /// Fowler–Noll–Vo hash implementation
    /// </summary>
    public static class FNV1A32Unsafe
    {
        public static unsafe uint ComputeHash(byte* data, int length)
        {
            uint hash = FNVConstants.FnvInit;

            for (int i = 0; i < length; i++)
            {
                hash ^= data[i];
                hash *= FNVConstants.FnvPrime;
            }

            return hash;
        }
    }
}