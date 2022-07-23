using Genbox.FastHash.FnvHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class FnvHashTests
{
    [Fact]
    public void Fnv1aHash32IndexTest()
    {
        uint val = 1u;
        for (int i = 1; i <= 32; i++)
        {
            uint h1 = Fnv1aHash32.ComputeHash(BitConverter.GetBytes(val));
            uint h2 = Fnv1aHash32.ComputeIndex(val);
            Assert.Equal(h1, h2);

            val <<= 1;
        }
    }
}