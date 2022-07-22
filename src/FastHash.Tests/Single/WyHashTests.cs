using System.Text;
using Genbox.FastHash.wyHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class WyHashTests
{
    [Theory]
    [InlineData(0, "", 0x42bc986dc5eec4d3)]
    [InlineData(1, "a", 0x84508dc903c31551)]
    [InlineData(2, "abc", 0x0bc54887cfc9ecb1)]
    [InlineData(3, "message digest", 0x6e2ff3298208a67c)]
    [InlineData(4, "abcdefghijklmnopqrstuvwxyz", 0x9a64e42e897195b9)]
    [InlineData(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 0x9199383239c32554)]
    [InlineData(6, "12345678901234567890123456789012345678901234567890123456789012345678901234567890", 0x7c1ccf6bba30f5a5)]
    public void TestVectors(int seed, string value, ulong hash)
    {
        ulong h = Wy3Hash64.ComputeHash(Encoding.ASCII.GetBytes(value), (ulong)seed);
        Assert.Equal(hash, h);
    }

    [Fact]
    public void wy3Hash64IndexTest()
    {
        ulong val = 1ul;
        for (int i = 1; i <= 64; i++)
        {
            ulong h1 = Wy3Hash64.ComputeHash(BitConverter.GetBytes(val));
            ulong h2 = Wy3Hash64.ComputeIndex(val);
            Assert.Equal(h1, h2);

            val <<= 1;
        }
    }
}