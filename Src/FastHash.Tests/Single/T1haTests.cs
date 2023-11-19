using Genbox.FastHash.T1haHash;

namespace Genbox.FastHash.Tests.Single;

public class T1haTests
{
    private static readonly ulong[] t1ha_refval_ia32aes_a =
    {
        0,
        0x772C7311BE32FF42, 0xB231AC660E5B23B5, 0x71F6DF5DA3B4F532, 0x555859635365F660,
        0xE98808F1CD39C626, 0x2EB18FAF2163BB09, 0x7B9DD892C8019C87, 0xE2B1431C4DA4D15A,
        0x1984E718A5477F70, 0x08DD17B266484F79, 0x4C83A05D766AD550, 0x92DCEBB131D1907D,
        0xD67BC6FC881B8549, 0xF6A9886555FBF66B, 0x6E31616D7F33E25E, 0x36E31B7426E3049D,
        0x4F8E4FAF46A13F5F, 0x03EB0CB3253F819F, 0x636A7769905770D2, 0x3ADF3781D16D1148,
        0x92D19CB1818BC9C2, 0x283E68F4D459C533, 0xFA83A8A88DECAA04, 0x8C6F00368EAC538C,
        0x7B66B0CF3797B322, 0x5131E122FDABA3FF, 0x6E59FF515C08C7A9, 0xBA2C5269B2C377B0,
        0xA9D24FD368FE8A2B, 0x22DB13D32E33E891, 0x7B97DFC804B876E5, 0xC598BDFCD0E834F9,
        0xB256163D3687F5A7, 0x66D7A73C6AEF50B3, 0xBB34C6A4396695D2, 0x7F46E1981C3256AD,
        0x4B25A9B217A6C5B4, 0x7A0A6BCDD2321DA9, 0x0A1F55E690A7B44E, 0x8F451A91D7F05244,
        0x624D5D3C9B9800A7, 0x09DDC2B6409DDC25, 0x3E155765865622B6, 0x96519FAC9511B381,
        0x512E58482FE4FBF0, 0x1AB260EA7D54AE1C, 0x67976F12CC28BBBD, 0x0607B5B2E6250156,
        0x7E700BEA717AD36E, 0x06A058D9D61CABB3, 0x57DA5324A824972F, 0x1193BA74DBEBF7E7,
        0xC18DC3140E7002D4, 0x9F7CCC11DFA0EF17, 0xC487D6C20666A13A, 0xB67190E4B50EF0C8,
        0xA53DAA608DF0B9A5, 0x7E13101DE87F9ED3, 0x7F8955AE2F05088B, 0x2DF7E5A097AD383F,
        0xF027683A21EA14B5, 0x9BB8AEC3E3360942, 0x92BE39B54967E7FE, 0x978C6D332E7AFD27,
        0xED512FE96A4FAE81, 0x9E1099B8140D7BA3, 0xDFD5A5BE1E6FE9A6, 0x1D82600E23B66DD4,
        0x3FA3C3B7EE7B52CE, 0xEE84F7D2A655EF4C, 0x2A4361EC769E3BEB, 0x22E4B38916636702,
        0x0063096F5D39A115, 0x6C51B24DAAFA5434, 0xBAFB1DB1B411E344, 0xFF529F161AE0C4B0,
        0x1290EAE3AC0A686F, 0xA7B0D4585447D1BE, 0xAED3D18CB6CCAD53, 0xFC73D46F8B41BEC6
    };

    private static readonly byte[] t1ha_test_pattern =
    {
        /* 8  */ 0, 1, 2, 3, 4, 5, 6, 7, 0xFF,
        /* 16 */ 0x7F, 0x3F, 0x1F, 0xF, 8, 16, 32, 64,
        /* 24 */ 0x80, 0xFE, 0xFC, 0xF8, 0xF0, 0xE0, 0xC0, 0xFD,
        /* 32 */ 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x55, 0xAA, 11,
        /* 40 */ 17, 19, 23, 29, 37, 42, 43, (byte)'a',
        /* 48 */ (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i',
        /* 56 */ (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'q',
        /* 62 */ (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x'
    };

    private static unsafe void probe(int testOffset, byte* data, int len, ulong seed)
    {
        ulong reference = t1ha_refval_ia32aes_a[testOffset];
        ulong actual = T1ha0Hash64.ComputeHash(data, (uint)len, seed);
        Assert.Equal(reference.ToString("X8"), actual.ToString("X8"));
    }

    [Fact]
    public unsafe void TestVectors()
    {
        int testOffset = 0;
        ulong seed = 1;

        fixed (byte* ptr = t1ha_test_pattern)
        {
            ulong zero = 0;
            probe(testOffset++, null, 0, zero); // empty-zero
            probe(testOffset++, null, 0, ~zero); // empty-all1
            probe(testOffset++, ptr, 64, zero); // bin64-zero

            for (int i = 1; i < 64; i++)
            {
                probe(testOffset++, ptr, i, seed);
                seed <<= 1;
            }

            seed = ~zero;
            for (int i = 1; i <= 7; i++)
            {
                seed <<= 1;
                probe(testOffset++, ptr + i, 64 - i, seed);
            }
        }

        byte[] pattern_long = new byte[512];

        for (int i = 0; i < pattern_long.Length; ++i)
            pattern_long[i] = (byte)i;

        fixed (byte* ptr = pattern_long)
        {
            for (int i = 0; i <= 7; i++)
            {
                probe(testOffset++, ptr + i, 128 + i * 17, seed);
            }
        }
    }
}