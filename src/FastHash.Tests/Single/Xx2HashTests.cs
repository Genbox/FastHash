using System.Text;
using Genbox.FastHash.xxHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class Xx2HashTests
{
    [Fact]
    public void TestCase1()
    {
        const string Test = "Hello World10";

        byte[] bytes = Encoding.ASCII.GetBytes(Test);

        ulong res = Xx2Hash64.ComputeHash(bytes);

        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                ulong res2 = Xx2Hash64Unsafe.ComputeHash(ptr, bytes.Length);

                Assert.Equal(BitConverter.ToString(BitConverter.GetBytes(res)), BitConverter.ToString(BitConverter.GetBytes(res2)));
            }
        }
    }

    [Fact]
    public void TestIndexVersion()
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