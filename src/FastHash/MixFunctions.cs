using System.Runtime.CompilerServices;

namespace Genbox.FastHash;

public static class MixFunctions
{
    #region Murmur

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Murmur_32(uint h)
    {
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Murmur_32_Seed(uint h, uint seed)
    {
        h += seed;
        h ^= h >> 16;
        h *= 0x85EBCA6BU;
        h ^= h >> 13;
        h *= 0xC2B2AE35U;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Murmur_32_SeedMix(uint h, uint seed)
    {
        h ^= h >> 16;
        h *= 0x85EBCA6BU + seed;
        h ^= h >> 13;
        h *= 0xC2B2AE35U;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur_64(ulong h)
    {
        h ^= h >> 33;
        h *= 0xff51afd7ed558ccd;
        h ^= h >> 33;
        h *= 0xc4ceb9fe1a85ec53;
        h ^= h >> 33;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= h >> 33;
        h *= 0xFF51AFD7ED558CCDUL;
        h ^= h >> 33;
        h *= 0xC4CEB9FE1A85EC53UL;
        h ^= h >> 33;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur_64_SeedMix(ulong h, ulong seed)
    {
        h ^= h >> 33;
        h *= 0xFF51AFD7ED558CCDUL + seed;
        h ^= h >> 33;
        h *= 0xC4CEB9FE1A85EC53UL;
        h ^= h >> 33;
        return h;
    }

    #endregion

    #region Murmur Variants

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong City_64_Seed(ulong h, ulong seed)
    {
        //IQV: In CityHash this method is called HashLen16
        return City_128_Seed(h, seed, 0x9ddfea08eb382d69UL);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong City_128_Seed(ulong h1, ulong h2, ulong seed)
    {
        //IQV: In CityHash this method is called HashLen16
        // Murmur-inspired hashing.
        ulong a = (h1 ^ h2) * seed;
        a ^= a >> 47;
        ulong b = (h2 ^ a) * seed;
        b ^= b >> 47;
        b *= seed;
        return b;
    }

    //http://jonkagstrom.com/mx3/mx3_rev2.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mx3_64(ulong h)
    {
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 29;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 29;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mx3_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 29;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 29;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mx3_64_SeedMix(ulong h, ulong seed)
    {
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556DUL + seed;
        h ^= h >> 29;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556DUL;
        h ^= h >> 29;
        return h;
    }

    //http://jonkagstrom.com/tuning-bit-mixers/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Xmx_64(ulong h)
    {
        h ^= h >> 23;
        h *= 0xFF51AFD7ED558CCDUL;
        h ^= h >> 23;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Xmx_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= h >> 23;
        h *= 0xFF51AFD7ED558CCDUL;
        h ^= h >> 23;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Xmx_64_SeedMix(ulong h, ulong seed)
    {
        h ^= h >> 23;
        h *= 0xFF51AFD7ED558CCDUL + seed;
        h ^= h >> 23;
        return h;
    }

    //http://mostlymangling.blogspot.com/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MoreMur_64(ulong h)
    {
        h ^= h >> 27;
        h *= 0x3C79AC492BA7B653UL;
        h ^= h >> 33;
        h *= 0x1C69B3F74AC4AE35UL;
        h ^= h >> 27;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MoreMur_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= h >> 27;
        h *= 0x3C79AC492BA7B653UL;
        h ^= h >> 33;
        h *= 0x1C69B3F74AC4AE35UL;
        h ^= h >> 27;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MoreMur_64_SeedMix(ulong h, ulong seed)
    {
        h ^= h >> 27;
        h *= 0x3C79AC492BA7B653UL + seed;
        h ^= h >> 33;
        h *= 0x1C69B3F74AC4AE35UL;
        h ^= h >> 27;
        return h;
    }

    //http://zimbry.blogspot.com/2011/09/better-bit-mixing-improving-on.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur14_64(ulong h)
    {
        h ^= h >> 30;
        h *= 0x4BE98134A5976FD3UL;
        h ^= h >> 29;
        h *= 0x3BC0993A5AD19A13UL;
        h ^= h >> 31;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur14_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= h >> 30;
        h *= 0x4BE98134A5976FD3UL;
        h ^= h >> 29;
        h *= 0x3BC0993A5AD19A13UL;
        h ^= h >> 31;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur14_64_SeedMix(ulong h, ulong seed)
    {
        h ^= h >> 30;
        h *= 0x4BE98134A5976FD3UL + seed;
        h ^= h >> 29;
        h *= 0x3BC0993A5AD19A13UL;
        h ^= h >> 31;
        return h;
    }

    #endregion

    #region Xxhash

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XXH2_32(uint h)
    {
        h ^= h >> 15;
        h *= 0x85EBCA77U;
        h ^= h >> 13;
        h *= 0xC2B2AE3DU;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XXH2_32_Seed(uint h, uint seed)
    {
        h += seed;
        h ^= h >> 15;
        h *= 0x85EBCA77U;
        h ^= h >> 13;
        h *= 0xC2B2AE3DU;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XXH2_32_SeedMix(uint h, uint seed)
    {
        h ^= h >> 15;
        h *= 0x85EBCA77U + seed;
        h ^= h >> 13;
        h *= 0xC2B2AE3DU;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH2_64(ulong h)
    {
        h ^= h >> 33;
        h *= 0xC2B2AE3D27D4EB4FUL;
        h ^= h >> 29;
        h *= 0x165667B19E3779F9UL;
        h ^= h >> 32;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH2_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= h >> 33;
        h *= 0xC2B2AE3D27D4EB4FUL;
        h ^= h >> 29;
        h *= 0x165667B19E3779F9UL;
        h ^= h >> 32;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH2_64_SeedMix(ulong h, ulong seed)
    {
        h ^= h >> 33;
        h *= 0xC2B2AE3D27D4EB4FUL + seed;
        h ^= h >> 29;
        h *= 0x165667B19E3779F9UL;
        h ^= h >> 32;
        return h;
    }

    #endregion

    #region Others

    /// <summary>https://github.com/ztanml/fast-hash</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong FastHash_64(ulong h)
    {
        h ^= h >> 23;
        h *= 0x2127599bf4325c37UL;
        h ^= h >> 47;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong FastHash_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= h >> 23;
        h *= 0x2127599bf4325c37UL;
        h ^= h >> 47;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong FastHash_64_SeedMix(ulong h, ulong seed)
    {
        h ^= h >> 23;
        h *= 0x2127599bf4325c37UL + seed;
        h ^= h >> 47;
        return h;
    }

    /// <summary>http://mostlymangling.blogspot.com/2020/01/nasam-not-another-strange-acronym-mixer.html</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Nasam_64(ulong h)
    {
        h ^= RotateRight(h, 25) ^ RotateRight(h, 47);
        h *= 0x9E6C63D0676A9A99UL;
        h ^= h >> 23 ^ h >> 51;
        h *= 0x9E6D62D06F6A9A9BUL;
        h ^= h >> 23 ^ h >> 51;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Nasam_64_Seed(ulong h, ulong seed)
    {
        h += seed;
        h ^= RotateRight(h, 25) ^ RotateRight(h, 47);
        h *= 0x9E6C63D0676A9A99UL;
        h ^= h >> 23 ^ h >> 51;
        h *= 0x9E6D62D06F6A9A9BUL;
        h ^= h >> 23 ^ h >> 51;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Nasam_64_SeedMix(ulong h, ulong seed)
    {
        h ^= RotateRight(h, 25) ^ RotateRight(h, 47);
        h *= 0x9E6C63D0676A9A99UL + seed;
        h ^= h >> 23 ^ h >> 51;
        h *= 0x9E6D62D06F6A9A9BUL;
        h ^= h >> 23 ^ h >> 51;
        return h;
    }

    #endregion
}