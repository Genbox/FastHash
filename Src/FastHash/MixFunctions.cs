﻿using System.Runtime.CompilerServices;

namespace Genbox.FastHash;

public static class MixFunctions
{
    #region Murmur - Austin Appleby

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Murmur_32(uint h)
    {
        h ^= h >> 16;
        h *= 0x85EBCA6B;
        h ^= h >> 13;
        h *= 0xC2B2AE35;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur_64(ulong h)
    {
        h ^= h >> 33;
        h *= 0xFF51AFD7ED558CCD;
        h ^= h >> 33;
        h *= 0xC4CEB9FE1A85EC53;
        h ^= h >> 33;
        return h;
    }

    #endregion

    #region Murmur variant - Jon Maiga

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I7_mrm_64(ulong h)
    {
        //x c2 mul 59 ror c2 mul
        h *= 0xBF58476D1CE4E5B9;
        h = RotateRight(h, 59);
        h *= 0x94D049BB133111EB;
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I8_mxm_64(ulong h)
    {
        //x c1 mul 56 xsr c2 mul
        h *= 0xBF58476D1CE4E5B9;
        h ^= h >> 56;
        h *= 0x94D049BB133111EB;
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I9_xmx_64(ulong h)
    {
        //x 23 xsr c3 mul 23 xsr
        h ^= h >> 23;
        h *= 0xFF51AFD7ED558CCD;
        h ^= h >> 23;
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I11_mxma_64(ulong h)
    {
        // x c3 mul 32 xsr c3 mul 32 asr
        h *= 0xFF51AFD7ED558CCD;
        h ^= h >> 32;
        h *= 0xFF51AFD7ED558CCD;
        h += h >> 32;
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I11_mxmx_64(ulong h)
    {
        // x c3 mul 47 xsr c1 mul 32 xsr
        h *= 0xFF51AFD7ED558CCD;
        h ^= h >> 47;
        h *= 0xBF58476D1CE4E5B9;
        h ^= h >> 32;
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I12_xmrx_64(ulong h)
    {
        // x c3 mul 47 xsr c1 mul 32 xsr
        h ^= h >> 32;
        h *= 0xFF51AFD7ED558CCD;
        h ^= RotateRight(h, 47) ^ RotateRight(h, 23);
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I13_mxmxm_64(ulong h)
    {
        // x c1 mul 32 xsr c2 mul 32 xsr c2 mul
        h *= 0xBF58476D1CE4E5B9;
        h ^= h >> 32;
        h *= 0x94D049BB133111EB;
        h ^= h >> 32;
        h *= 0x94D049BB133111EB;
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I14_mxrmx_64(ulong h)
    {
        // x c2 mul 56 32 xrr c3 mul 23 xsr
        h *= 0x94D049BB133111EB;
        h ^= RotateRight(h, 56) ^ RotateRight(h, 32);
        h *= 0xFF51AFD7ED558CCD;
        h ^= h >> 23;
        return h;
    }

    //https://jonkagstrom.com/bit-mixer-construction/index.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong I15_mxmxmx_64(ulong x)
    {
        x *= 0xBF58476D1CE4E5B9;
        x ^= x >> 32;
        x *= 0x94D049BB133111EB;
        x ^= x >> 32;
        x *= 0xFF51AFD7ED558CCD;
        x ^= x >> 32;
        return x;
    }

    //https://jonkagstrom.com/mx3/mx3_rev2.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mx2_64(ulong h)
    {
        h ^= h >> 32;
        h *= 0xe9846af9b1a615d;
        h ^= h >> 32;
        h *= 0xe9846af9b1a615d;
        h ^= h >> 28;
        return h;
    }

    // http://jonkagstrom.com/mx3/mx3_rev2.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mx3_64(ulong h)
    {
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556D;
        h ^= h >> 29;
        h *= 0xBEA225F9EB34556D;
        h ^= h >> 32;
        h *= 0xBEA225F9EB34556D;
        h ^= h >> 29;
        return h;
    }

    #endregion

    #region Murmur variant - Pelle Evensen

    // https://mostlymangling.blogspot.com/2019/01/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Rrmxmx(ulong h)
    {
        h ^= RotateRight(h, 49) ^ RotateRight(h, 24);
        h *= 0x9FB21C651E98DF25L;
        h ^= h >> 28;
        h *= 0x9FB21C651E98DF25L;
        return h ^ h >> 28;
    }

    // https://mostlymangling.blogspot.com/2019/12/?m=0
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MoreMur_64(ulong h)
    {
        h ^= h >> 27;
        h *= 0x3C79AC492BA7B653;
        h ^= h >> 33;
        h *= 0x1C69B3F74AC4AE35;
        h ^= h >> 27;
        return h;
    }

    // http://mostlymangling.blogspot.com/2020/01/nasam-not-another-strange-acronym-mixer.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Nasam_64(ulong h)
    {
        h ^= RotateRight(h, 25) ^ RotateRight(h, 47);
        h *= 0x9E6C63D0676A9A99;
        h ^= h >> 23 ^ h >> 51;
        h *= 0x9E6D62D06F6A9A9B;
        h ^= h >> 23 ^ h >> 51;
        return h;
    }

    #endregion

    #region Murmur variant - David Stafford

    // http://zimbry.blogspot.com/2011/09/better-bit-mixing-improving-on.html

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix01(ulong h)
    {
        h ^= h >> 31;
        h *= 0x7FB5D329728EA185;
        h ^= h >> 27;
        h *= 0x81DADEF4BC2DD44D;
        h ^= h >> 33;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix02(ulong h)
    {
        h ^= h >> 33;
        h *= 0x64DD81482CBD31D7;
        h ^= h >> 31;
        h *= 0xE36AA5C613612997;
        h ^= h >> 31;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix03(ulong h)
    {
        h ^= h >> 31;
        h *= 0x99BCF6822B23CA35;
        h ^= h >> 30;
        h *= 0x14020A57ACCED8B7;
        h ^= h >> 33;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix04(ulong h)
    {
        h ^= h >> 33;
        h *= 0x62A9D9ED799705F5;
        h ^= h >> 28;
        h *= 0xCB24D0A5C88C35B3;
        h ^= h >> 32;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix05(ulong h)
    {
        h ^= h >> 31;
        h *= 0x79C135C1674B9ADD;
        h ^= h >> 29;
        h *= 0x54C77C86F6913E45;
        h ^= h >> 30;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix06(ulong h)
    {
        h ^= h >> 31;
        h *= 0x69B0BC90BD9A8C49;
        h ^= h >> 27;
        h *= 0x3D5E661A2A77868D;
        h ^= h >> 30;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix07(ulong h)
    {
        h ^= h >> 30;
        h *= 0x16A6AC37883AF045;
        h ^= h >> 26;
        h *= 0xCC9C31A4274686A5;
        h ^= h >> 32;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix08(ulong h)
    {
        h ^= h >> 30;
        h *= 0x294AA62849912F0B;
        h ^= h >> 28;
        h *= 0x0A9BA9C8A5B15117;
        h ^= h >> 31;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix09(ulong h)
    {
        h ^= h >> 32;
        h *= 0x4CD6944C5CC20B6D;
        h ^= h >> 29;
        h *= 0xFC12C5B19D3259E9;
        h ^= h >> 32;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix10(ulong h)
    {
        h ^= h >> 30;
        h *= 0xE4C7E495F4C683F5;
        h ^= h >> 32;
        h *= 0xFDA871BAEA35A293;
        h ^= h >> 33;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix11(ulong h)
    {
        h ^= h >> 27;
        h *= 0x97D461A8B11570D9;
        h ^= h >> 28;
        h *= 0x02271EB7C6C4CD6B;
        h ^= h >> 32;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix12(ulong h)
    {
        h ^= h >> 29;
        h *= 0x3CD0EB9D47532DFB;
        h ^= h >> 26;
        h *= 0x63660277528772BB;
        h ^= h >> 33;
        return h;
    }

    // This is the variant used in SplitMix64
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix13(ulong h)
    {
        h ^= h >> 30;
        h *= 0xBF58476D1CE4E5B9;
        h ^= h >> 27;
        h *= 0x94D049BB133111EB;
        h ^= h >> 31;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mix14(ulong h)
    {
        h ^= h >> 30;
        h *= 0x4BE98134A5976FD3;
        h ^= h >> 29;
        h *= 0x3BC0993A5AD19A13;
        h ^= h >> 31;
        return h;
    }

    #endregion

    #region Murmur variant - Others

    // https://github.com/ztanml/fast-hash
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong FastHash_64(ulong h)
    {
        h ^= h >> 23;
        h *= 0x2127599BF4325C37;
        h ^= h >> 47;
        return h;
    }

    // https://dl.acm.org/doi/pdf/10.1145/3485525
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Lea_64(ulong h)
    {
        h ^= h >> 32;
        h *= 0xDABA0B6EB09322E3;
        h ^= h >> 32;
        h *= 0xDABA0B6EB09322E3;
        return h ^ (h >> 32);
    }

    // https://gist.github.com/degski/6e2069d6035ae04d5d6f64981c995ec2
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Degski_32(uint h)
    {
        h ^= h >> 16;
        h *= 0x45D9F3BU;
        h ^= h >> 16;
        h *= 0x45D9F3BU;
        h ^= h >> 16;
        return h;
    }

    // https://gist.github.com/degski/6e2069d6035ae04d5d6f64981c995ec2
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Degski_64(ulong h)
    {
        h ^= h >> 32;
        h *= 0xD6E8FEB86659FD93;
        h ^= h >> 32;
        h *= 0xD6E8FEB86659FD93;
        return h ^ (h >> 32);
    }

    // https://github.com/skeeto/hash-prospector/issues/23
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Fp64_32(uint h)
    {
        h ^= h >> 15;
        h = (h | 1u) ^ (h * h);
        h ^= h >> 17;
        h *= 0x9E3779B9u;
        h ^= h >> 13;
        return h;
    }

    #endregion

    #region Xxhash

    // https://github.com/Cyan4973/xxHash/tree/dev
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

    // https://github.com/Cyan4973/xxHash/tree/dev
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH2_64(ulong h)
    {
        h ^= h >> 33;
        h *= 0xC2B2AE3D27D4EB4F;
        h ^= h >> 29;
        h *= 0x165667B19E3779F9;
        h ^= h >> 32;
        return h;
    }

    // https://github.com/Cyan4973/xxHash/tree/dev
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH3_64(ulong h)
    {
        h ^= h >> 37;
        h *= 0x165667919E3779F9;
        h ^= h >> 32;
        return h;
    }

    #endregion

    #region Others

    // TheIronBorn
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LowBias32(uint h)
    {
        h ^= h >> 16;
        h *= 0x7feb352d;
        h ^= h >> 15;
        h *= 0x846ca68b;
        h ^= h >> 16;
        return h;
    }

    // https://github.com/skeeto/hash-prospector/tree/master
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Triple32(uint h)
    {
        h ^= h >> 17;
        h *= 0xed5ad4bb;
        h ^= h >> 11;
        h *= 0xac4c1b51;
        h ^= h >> 15;
        h *= 0x31848bab;
        h ^= h >> 14;
        return h;
    }

    // Tommy Ettinger
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Ettinger_64(ulong h)
    {
        h ^= 0xDB4F0B9175AE2165;
        h *= 0x4823A80B2006E21B;
        h ^= RotateLeft(h, 52) ^ RotateLeft(h, 21) ^ 0x9E3779B97F4A7C15;
        h *= 0x81383173;
        h ^= h >> 28;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CityMix_64(ulong h)
    {
        ulong a = (h ^ 0x9DDFEA08EB382D69) * 0x9DDFEA08EB382D69;
        a ^= a >> 47;
        ulong b = (0x9DDFEA08EB382D69 ^ a) * 0x9DDFEA08EB382D69;
        b ^= b >> 47;
        b *= 0x9DDFEA08EB382D69;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Umash_64(ulong h)
    {
        return h ^ RotateLeft(h, 8) ^ RotateLeft(h, 33);
    }

    #endregion
}