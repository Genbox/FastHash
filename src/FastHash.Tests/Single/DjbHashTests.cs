using Genbox.FastHash.DjbHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class DjbHashTests
{
    [Fact]
    public void Djb2Hash32IndexTest()
    {
        uint val = 1u;
        for (int i = 1; i <= 32; i++)
        {
            uint h1 = Djb2Hash32.ComputeHash(BitConverter.GetBytes(val));
            uint h2 = Djb2Hash32.ComputeIndex(val);
            Assert.Equal(h1, h2);

            val <<= 1;
        }
    }
}