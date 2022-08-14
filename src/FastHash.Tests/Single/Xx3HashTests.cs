using Genbox.FastHash.XxHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class Xx3HashTests
{
    private const uint PRIME32 = 2654435761U;
    private const ulong PRIME64 = 11400714785074694797UL;
    private readonly byte[] _sanityBuffer;

    private static readonly (int, ulong, ulong)[] _testVectors64 =
    {
        (0, 0, 0x2D06800538D394C2UL), /* empty string */
        (0, PRIME64, 0xA8A6B918B2F0364AUL), /* empty string */
        (1, 0, 0xC44BDFF4074EECDBUL), /*  1 -  3 */
        (1, PRIME64, 0x032BE332DD766EF8UL), /*  1 -  3 */
        (6, 0, 0x27B56A84CD2D7325UL), /*  4 -  8 */
        (6, PRIME64, 0x84589C116AB59AB9UL), /*  4 -  8 */
        (12, 0, 0xA713DAF0DFBB77E7UL), /*  9 - 16 */
        (12, PRIME64, 0xE7303E1B2336DE0EUL), /*  9 - 16 */
        (24, 0, 0xA3FE70BF9D3510EBUL), /* 17 - 32 */
        (24, PRIME64, 0x850E80FC35BDD690UL), /* 17 - 32 */
        (48, 0, 0x397DA259ECBA1F11UL), /* 33 - 64 */
        (48, PRIME64, 0xADC2CBAA44ACC616UL), /* 33 - 64 */
        (80, 0, 0xBCDEFBBB2C47C90AUL), /* 65 - 96 */
        (80, PRIME64, 0xC6DD0CB699532E73UL), /* 65 - 96 */
        (195, 0, 0xCD94217EE362EC3AUL), /* 129-240 */
        (195, PRIME64, 0xBA68003D370CB3D9UL), /* 129-240 */

        (403, 0, 0xCDEB804D65C6DEA4UL), /* one block, last stripe is overlapping */
        (403, PRIME64, 0x6259F6ECFD6443FDUL), /* one block, last stripe is overlapping */
        (512, 0, 0x617E49599013CB6BUL), /* one block, finishing at stripe boundary */
        (512, PRIME64, 0x3CE457DE14C27708UL), /* one block, finishing at stripe boundary */
        (2048, 0, 0xDD59E2C3A5F038E0UL), /* 2 blocks, finishing at block boundary */
        (2048, PRIME64, 0x66F81670669ABABCUL), /* 2 blocks, finishing at block boundary */
        (2240, 0, 0x6E73A90539CF2948UL), /* 3 blocks, finishing at stripe boundary */
        (2240, PRIME64, 0x757BA8487D1B5247UL), /* 3 blocks, finishing at stripe boundary */
        (2367, 0, 0xCB37AEB9E5D361EDUL), /* 3 blocks, last stripe is overlapping */
        (2367, PRIME64, 0xD2DB3415B942B42AUL) /* 3 blocks, last stripe is overlapping */
    };

    private static readonly (int, ulong, (ulong, ulong))[] _testVectors128 =
    {
        (0, 0, (0x6001C324468D497FUL, 0x99AA06D3014798D8UL)), /* empty string */
        (0, PRIME32, (0x5444F7869C671AB0UL, 0x92220AE55E14AB50UL)), /* empty string */
        (1, 0, (0xC44BDFF4074EECDBUL, 0xA6CD5E9392000F6AUL)), /*  1 -  3 */
        (1, PRIME32, (0xB53D5557E7F76F8DUL, 0x89B99554BA22467CUL)), /*  1 -  3 */
        (6, 0, (0x3E7039BDDA43CFC6UL, 0x082AFE0B8162D12AUL)), /*  4 -  8 */
        (6, PRIME32, (0x269D8F70BE98856EUL, 0x5A865B5389ABD2B1UL)), /*  4 -  8 */
        (12, 0, (0x061A192713F69AD9UL, 0x6E3EFD8FC7802B18UL)), /*  9 - 16 */
        (12, PRIME32, (0x9BE9F9A67F3C7DFBUL, 0xD7E09D518A3405D3UL)), /*  9 - 16 */
        (24, 0, (0x1E7044D28B1B901DUL, 0x0CE966E4678D3761UL)), /* 17 - 32 */
        (24, PRIME32, (0xD7304C54EBAD40A9UL, 0x3162026714A6A243UL)), /* 17 - 32 */
        (48, 0, (0xF942219AED80F67BUL, 0xA002AC4E5478227EUL)), /* 33 - 64 */
        (48, PRIME32, (0x7BA3C3E453A1934EUL, 0x163ADDE36C072295UL)), /* 33 - 64 */
        (81, 0, (0x5E8BAFB9F95FB803UL, 0x4952F58181AB0042UL)), /* 65 - 96 */
        (81, PRIME32, (0x703FBB3D7A5F755CUL, 0x2724EC7ADC750FB6UL)), /* 65 - 96 */
        (222, 0, (0xF1AEBD597CEC6B3AUL, 0x337E09641B948717UL)), /* 129-240 */
        (222, PRIME32, (0xAE995BB8AF917A8DUL, 0x91820016621E97F1UL)), /* 129-240 */

        (403, 0, (0xCDEB804D65C6DEA4UL, 0x1B6DE21E332DD73DUL)), /* one block, last stripe is overlapping */
        (403, PRIME64, (0x6259F6ECFD6443FDUL, 0xBED311971E0BE8F2UL)), /* one block, last stripe is overlapping */
        (512, 0, (0x617E49599013CB6BUL, 0x18D2D110DCC9BCA1UL)), /* one block, finishing at stripe boundary */
        (512, PRIME64, (0x3CE457DE14C27708UL, 0x925D06B8EC5B8040UL)), /* one block, finishing at stripe boundary */
        (2048, 0, (0xDD59E2C3A5F038E0UL, 0xF736557FD47073A5UL)), /* 2 blocks, finishing at block boundary */
        (2048, PRIME32, (0x230D43F30206260BUL, 0x7FB03F7E7186C3EAUL)), /* 2 blocks, finishing at block boundary */
        (2240, 0, (0x6E73A90539CF2948UL, 0xCCB134FBFA7CE49DUL)), /* 3 blocks, finishing at stripe boundary */
        (2240, PRIME32, (0xED385111126FBA6FUL, 0x50A1FE17B338995FUL)), /* 3 blocks, finishing at stripe boundary */
        (2367, 0, (0xCB37AEB9E5D361EDUL, 0xE89C0F6FF369B427UL)), /* 3 blocks, last stripe is overlapping */
        (2367, PRIME32, (0x6F5360AE69C2F406UL, 0xD23AAE4B76C31ECBUL)) /* 3 blocks, last stripe is overlapping */
    };

    public Xx3HashTests()
    {
        _sanityBuffer = new byte[2367];

        ulong byteGen = PRIME32;
        for (int i = 0; i < _sanityBuffer.Length; i++)
        {
            _sanityBuffer[i] = (byte)(byteGen >> 56);
            byteGen = unchecked(byteGen * PRIME64);
        }
    }

    [Fact]
    public unsafe void Xx3Hash64TestVectors()
    {
        for (int i = 0; i < _testVectors64.Length; i++)
        {
            (int len, ulong seed, ulong result) = _testVectors64[i];
            Assert.Equal(result, Xx3Hash64.ComputeHash(_sanityBuffer[..len], seed));

            fixed (byte* ptr = _sanityBuffer[..len])
                Assert.Equal(result, Xx3Hash64Unsafe.ComputeHash(ptr, len, seed));
        }
    }

    [Fact]
    public unsafe void Xx3Hash128TestVectors()
    {
        for (int i = 0; i < _testVectors128.Length; i++)
        {
            (int len, ulong seed, (ulong low, ulong high)) = _testVectors128[i];

            Uint128 res = Xx3Hash128.ComputeHash(_sanityBuffer[..len], seed);

            Assert.Equal(low, res.Low);
            Assert.Equal(high, res.High);

            fixed (byte* ptr = _sanityBuffer[..len])
            {
                Uint128 res2 = Xx3Hash128Unsafe.ComputeHash(ptr, len, seed);
                Assert.Equal(low, res2.Low);
                Assert.Equal(high, res2.High);
            }
        }
    }

    [Fact]
    public void TestLargeInput64Simd()
    {
        byte[] bytes = new byte[1024];
        new Random(42).NextBytes(bytes);

        ulong expected = 4085474917644329103;
        ulong actual = Xx3Hash64.ComputeHash(bytes);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestLargeInput128Simd()
    {
        byte[] bytes = new byte[1024];
        new Random(42).NextBytes(bytes);

        ulong expectedH = 9749042676328415322;
        ulong expectedL = 4085474917644329103;

        Uint128 actual = Xx3Hash128.ComputeHash(bytes);

        Assert.Equal(expectedH, actual.High);
        Assert.Equal(expectedL, actual.Low);
    }
}