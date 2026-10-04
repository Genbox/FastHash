// C# port of google/farmhash 1.1.0 (0d859a8). Copyright (c) 2014 Google, Inc.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.FarmHash;

internal static class FarmHashConstants
{
    // Some primes between 2^63 and 2^64 for various uses.
    internal const ulong K0 = 0xc3a5c85c97cb3127U;
    internal const ulong K1 = 0xb492b66fbe98f273U;
    internal const ulong K2 = 0x9ae16a3b2f90404fU;

    // Magic numbers for 32-bit hashing. Copied from Murmur3.
    internal const uint C1 = 0xcc9e2d51;
    internal const uint C2 = 0x1b873593;
}