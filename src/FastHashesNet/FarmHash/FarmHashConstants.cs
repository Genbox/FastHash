namespace Genbox.FastHashesNet.FarmHash;

internal static class FarmHashConstants
{
    // Some primes between 2^63 and 2^64 for various uses.
    internal const ulong k0 = 0xc3a5c85c97cb3127U;
    internal const ulong k1 = 0xb492b66fbe98f273U;
    internal const ulong k2 = 0x9ae16a3b2f90404fU;

    // Magic numbers for 32-bit hashing.  Copied from Murmur3.
    internal const uint c1 = 0xcc9e2d51;
    internal const uint c2 = 0x1b873593;
}