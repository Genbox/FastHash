using Genbox.FastHash.xxHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class xx3HashTests
{
    [Fact]
    public void TestLargeInput64Simd()
    {
        byte[] bytes = new byte[1024];
        new Random(42).NextBytes(bytes);

        ulong expected = 4085474917644329103;
        ulong actual = xx3Hash64.ComputeHash(bytes);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestLargeInput128Simd()
    {
        byte[] bytes = new byte[1024];
        new Random(42).NextBytes(bytes);

        ulong expectedH = 9749042676328415322;
        ulong expectedL = 4085474917644329103;

        Uint128 actual = xx3Hash128.ComputeHash(bytes);

        Assert.Equal(expectedH, actual.High);
        Assert.Equal(expectedL, actual.Low);
    }
}