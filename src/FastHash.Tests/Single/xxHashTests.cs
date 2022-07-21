using System.Text;
using Genbox.FastHash.xxHash;
using Xunit;

namespace Genbox.FastHash.Tests.Single;

public class xxHashTests
{
    [Fact]
    public void TestCase1()
    {
        const string Test = "Hello World10";

        byte[] bytes = Encoding.ASCII.GetBytes(Test);

        ulong res = xxHash64.ComputeHash(bytes);

        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                ulong res2 = xxHash64Unsafe.ComputeHash(ptr, bytes.Length);

                Assert.Equal(BitConverter.ToString(BitConverter.GetBytes(res)), BitConverter.ToString(BitConverter.GetBytes(res2)));
            }
        }
    }
}