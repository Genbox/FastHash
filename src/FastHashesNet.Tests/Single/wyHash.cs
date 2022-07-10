using System.Collections.Generic;
using System.Text;
using FastHashesNet.wyHash;
using Xunit;

namespace FastHashesNet.Tests.Single;

public class wyHashTests
{
    private static readonly ulong[] _defaultSecret = { 0xa0761d6478bd642ful, 0xe7037ed1a0b428dbul, 0x8ebc6af09c88c6e3ul, 0x589965cc75374cc3ul };

    [Theory]
    [InlineData(1, "a", 0x84508dc903c31551)]
    [InlineData(2, "abc", 0xbc54887cfc9ecb1)]
    [InlineData(3, "message digest", 0xadc146444841c430)]
    [InlineData(4, "abcdefghijklmnopqrstuvwxyz", 0x9a64e42e897195b9)]
    [InlineData(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 0x9199383239c32554)]
    public void TestVectors(int index, string value, ulong hash)
    {
        unsafe
        {
            fixed (byte* ptr = Encoding.ASCII.GetBytes(value))
            fixed (ulong* secretPtr = _defaultSecret)
            {
                ulong h = wyHash64Unsafe.ComputeHash(ptr, (ulong)value.Length, (ulong)index, secretPtr);
                Assert.Equal(hash, h);
            }
        }
    }
}