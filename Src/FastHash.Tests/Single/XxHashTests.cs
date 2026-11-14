using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Tests.Single;

public class XxHashTests
{
    private const uint PRIME32 = 2654435761U;

    //Source: https://github.com/Cyan4973/xxHash/blob/dev/cli/xsum_sanity_check.c#L99
    private static readonly (int len, uint seed, uint result)[] _testVectors32 =
    [
        (0, 0, 0x02CC5D05U),
        (0, PRIME32, 0x36B78AE7U),
        (1, 0, 0xCF65B03EU),
        (1, PRIME32, 0xB4545AA4U),
        (14, 0, 0x1208E7E2U),
        (14, PRIME32, 0x6AF1D1FEU),
        (222, 0, 0x5BD11DBDU),
        (222, PRIME32, 0x58803C5FU)
    ];

    private static readonly (int, uint, ulong)[] _testVectors64 =
    [
        (0, 0, 0xEF46DB3751D8E999UL),
        (0, PRIME32, 0xAC75FDA2929B17EFUL),
        (1, 0, 0xE934A84ADB052768UL),
        (1, PRIME32, 0x5014607643A9B4C3UL),
        (4, 0, 0x9136A0DCA57457EEUL),
        (14, 0, 0x8282DCC4994E35C8UL),
        (14, PRIME32, 0xC3BD6BF63DEB6DF0UL),
        (222, 0, 0xB641AE8CB691C174UL),
        (222, PRIME32, 0x20CB8AB7AE10C14AUL)
    ];
    private static readonly byte[] _sanityBuffer = TestData.XxSanity;

    [Fact]
    public void XxHash32TestVectors()
    {
        for (int i = 0; i < _testVectors32.Length; i++)
        {
            (int len, uint seed, uint result) = _testVectors32[i];
            Assert.Equal(result, XxHash32.ComputeHash(_sanityBuffer.AsSpan(0, len), seed));

        }
    }

    [Fact]
    public void XxHash64TestVectors()
    {
        for (int i = 0; i < _testVectors64.Length; i++)
        {
            (int len, uint seed, ulong result) = _testVectors64[i];
            Assert.Equal(XxHash64.ComputeHash(_sanityBuffer.AsSpan(0, len), seed), result);

        }
    }
}