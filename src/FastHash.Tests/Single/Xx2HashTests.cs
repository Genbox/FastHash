using Genbox.FastHash.xxHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class Xx2HashTests
{
    [Fact]
    public void Xx2HashIndexTest()
    {
        ulong val = 1ul;
        for (int i = 1; i <= 64; i++)
        {
            ulong h1 = Xx2Hash64.ComputeHash(BitConverter.GetBytes(val));
            ulong h2 = Xx2Hash64.ComputeIndex(val);
            Assert.Equal(h1, h2);

            val <<= 1;
        }
    }
}