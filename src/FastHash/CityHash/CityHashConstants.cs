namespace Genbox.FastHash.CityHash;

public static class CityHashConstants
{
    // Some primes between 2^63 and 2^64 for various uses.
    public const ulong k0 = 0xc3a5c85c97cb3127UL;
    public const ulong k1 = 0xb492b66fbe98f273UL;
    public const ulong k2 = 0x9ae16a3b2f90404fUL;

    // Magic numbers for 32-bit hashing. Copied from Murmur3.
    public const uint c1 = 0xcc9e2d51;
    public const uint c2 = 0x1b873593;
}