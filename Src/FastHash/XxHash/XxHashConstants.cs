// C# port of Cyan4973/xxHash 0.8.3 (e626a72). Copyright (c) 2012-2021 Yann Collet.
// Distributed under the BSD 2-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.XxHash;

internal static class XxHashConstants
{
    internal const int MIDSIZE_MAX = 240;
    internal const int MIDSIZE_STARTOFFSET = 3;
    internal const int MIDSIZE_LASTOFFSET = 17;

    internal const int ACC_SIZE = 64;
    internal const int ACC_NB = STRIPE_LEN / 8;
    internal const int STRIPE_LEN = 64;

    internal const int SECRET_SIZE_MIN = 136;
    internal const int SECRET_DEFAULT_SIZE = 192;
    internal const int SECRET_CONSUME_RATE = 8;
    internal const int SECRET_MERGEACCS_START = 11;
    internal const int SECRET_LASTACC_START = 7;

    internal const byte MM_SHUFFLE_0_3_0_1 = 0b0011_0001;
    internal const byte MM_SHUFFLE_1_0_3_2 = 0b0100_1110;

    internal const ulong SECRET_08_16_XOR = 0xc73ab174c5ecd5a2UL;
    internal const ulong SECRET_16_24_XOR = 0xc4f023344dc994acUL;

    internal const uint PRIME32_1 = 2654435761U;
    internal const uint PRIME32_2 = 2246822519U;
    internal const uint PRIME32_3 = 3266489917U;
    internal const uint PRIME32_4 = 668265263U;
    internal const uint PRIME32_5 = 374761393U;

    internal const ulong PRIME64_1 = 11400714785074694791UL;
    internal const ulong PRIME64_2 = 14029467366897019727UL;
    internal const ulong PRIME64_3 = 1609587929392839161UL;
    internal const ulong PRIME64_4 = 9650029242287828579UL;
    internal const ulong PRIME64_5 = 2870177450012600261UL;

    // [FixedAddressValueType]
    // internal static readonly Vector256<uint> M256i_XXH_PRIME32_1 = Vector256.Create(PRIME32_1);
    //
    // [FixedAddressValueType]
    // internal static readonly Vector128<uint> M128i_XXH_PRIME32_1 = Vector128.Create(PRIME32_1);

    // Little-endian words of kSecret, named by byte offset, for the short and medium paths.
    internal const ulong SECRET64_000 = 0xBE4BA423396CFEB8UL;
    internal const ulong SECRET64_003 = 0x81017CBE4BA42339UL;
    internal const ulong SECRET64_008 = 0x1CAD21F72C81017CUL;
    internal const ulong SECRET64_011 = 0x6DD4DE1CAD21F72CUL;
    internal const ulong SECRET64_016 = 0xDB979083E96DD4DEUL;
    internal const ulong SECRET64_019 = 0xA44072DB979083E9UL;
    internal const ulong SECRET64_024 = 0x1F67B3B7A4A44072UL;
    internal const ulong SECRET64_027 = 0xE679CB1F67B3B7A4UL;
    internal const ulong SECRET64_032 = 0x78E5C0CC4EE679CBUL;
    internal const ulong SECRET64_035 = 0xD05A8278E5C0CC4EUL;
    internal const ulong SECRET64_040 = 0x2172FFCC7DD05A82UL;
    internal const ulong SECRET64_043 = 0x4608B82172FFCC7DUL;
    internal const ulong SECRET64_048 = 0x8E2443F7744608B8UL;
    internal const ulong SECRET64_051 = 0x9035E08E2443F774UL;
    internal const ulong SECRET64_056 = 0x4C263A81E69035E0UL;
    internal const ulong SECRET64_059 = 0x52283C4C263A81E6UL;
    internal const ulong SECRET64_064 = 0xCB00C391BB52283CUL;
    internal const ulong SECRET64_067 = 0x65D088CB00C391BBUL;
    internal const ulong SECRET64_072 = 0xA32E531B8B65D088UL;
    internal const ulong SECRET64_075 = 0x486471A32E531B8BUL;
    internal const ulong SECRET64_080 = 0x4EF90DA297486471UL;
    internal const ulong SECRET64_083 = 0xEF19384EF90DA297UL;
    internal const ulong SECRET64_088 = 0xD8ACDEA946EF1938UL;
    internal const ulong SECRET64_091 = 0x76FAA8D8ACDEA946UL;
    internal const ulong SECRET64_096 = 0x3F349CE33F76FAA8UL;
    internal const ulong SECRET64_099 = 0xBBDCF93F349CE33FUL;
    internal const ulong SECRET64_103 = 0x4F0BC7C7BBDCF93FUL;
    internal const ulong SECRET64_104 = 0x1D4F0BC7C7BBDCF9UL;
    internal const ulong SECRET64_107 = 0xE0518A1D4F0BC7C7UL;
    internal const ulong SECRET64_111 = 0x59B4CD4BE0518A1DUL;
    internal const ulong SECRET64_112 = 0x3159B4CD4BE0518AUL;
    internal const ulong SECRET64_119 = 0x7378D9C97E9FC831UL;
    internal const ulong SECRET64_120 = 0x647378D9C97E9FC8UL;
    internal const ulong SECRET64_127 = 0xEBD33483ACC5EA64UL;
    internal const uint SECRET32_000 = 0x396CFEB8U;
    internal const uint SECRET32_004 = 0xBE4BA423U;
    internal const uint SECRET32_008 = 0x2C81017CU;
    internal const uint SECRET32_012 = 0x1CAD21F7U;

    internal static readonly byte[] kSecret =
    [
        0xb8, 0xfe, 0x6c, 0x39, 0x23, 0xa4, 0x4b, 0xbe, 0x7c, 0x01, 0x81, 0x2c, 0xf7, 0x21, 0xad, 0x1c,
        0xde, 0xd4, 0x6d, 0xe9, 0x83, 0x90, 0x97, 0xdb, 0x72, 0x40, 0xa4, 0xa4, 0xb7, 0xb3, 0x67, 0x1f,
        0xcb, 0x79, 0xe6, 0x4e, 0xcc, 0xc0, 0xe5, 0x78, 0x82, 0x5a, 0xd0, 0x7d, 0xcc, 0xff, 0x72, 0x21,
        0xb8, 0x08, 0x46, 0x74, 0xf7, 0x43, 0x24, 0x8e, 0xe0, 0x35, 0x90, 0xe6, 0x81, 0x3a, 0x26, 0x4c,
        0x3c, 0x28, 0x52, 0xbb, 0x91, 0xc3, 0x00, 0xcb, 0x88, 0xd0, 0x65, 0x8b, 0x1b, 0x53, 0x2e, 0xa3,
        0x71, 0x64, 0x48, 0x97, 0xa2, 0x0d, 0xf9, 0x4e, 0x38, 0x19, 0xef, 0x46, 0xa9, 0xde, 0xac, 0xd8,
        0xa8, 0xfa, 0x76, 0x3f, 0xe3, 0x9c, 0x34, 0x3f, 0xf9, 0xdc, 0xbb, 0xc7, 0xc7, 0x0b, 0x4f, 0x1d,
        0x8a, 0x51, 0xe0, 0x4b, 0xcd, 0xb4, 0x59, 0x31, 0xc8, 0x9f, 0x7e, 0xc9, 0xd9, 0x78, 0x73, 0x64,
        0xea, 0xc5, 0xac, 0x83, 0x34, 0xd3, 0xeb, 0xc3, 0xc5, 0x81, 0xa0, 0xff, 0xfa, 0x13, 0x63, 0xeb,
        0x17, 0x0d, 0xdd, 0x51, 0xb7, 0xf0, 0xda, 0x49, 0xd3, 0x16, 0x55, 0x26, 0x29, 0xd4, 0x68, 0x9e,
        0x2b, 0x16, 0xbe, 0x58, 0x7d, 0x47, 0xa1, 0xfc, 0x8f, 0xf8, 0xb8, 0xd1, 0x7a, 0xd0, 0x31, 0xce,
        0x45, 0xcb, 0x3a, 0x8f, 0x95, 0x16, 0x04, 0x28, 0xaf, 0xd7, 0xfb, 0xca, 0xbb, 0x4b, 0x40, 0x7e
    ];

    internal static readonly ulong[] INIT_ACC =
    [
        PRIME32_3, PRIME64_1, PRIME64_2, PRIME64_3,
        PRIME64_4, PRIME32_2, PRIME64_5, PRIME32_1
    ];
}