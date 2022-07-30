namespace Genbox.FastHash.CityHash;

internal static class CityHashConstants
{
    // Some primes between 2^63 and 2^64 for various uses.
    internal const ulong K0 = 0xc3a5c85c97cb3127UL;
    internal const ulong K1 = 0xb492b66fbe98f273UL;
    internal const ulong K2 = 0x9ae16a3b2f90404fUL;

    // Magic numbers for 32-bit hashing. Copied from Murmur3.
    internal const uint C1 = 0xcc9e2d51;
    internal const uint C2 = 0x1b873593;
}