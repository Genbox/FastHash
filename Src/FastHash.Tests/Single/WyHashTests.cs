using System.Buffers.Binary;
using System.Text;
using Genbox.FastHash.WyHash;

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

    [Theory]
    [InlineData(0, "", 0x0bc98efd7661a7a1)]
    [InlineData(1, "a", 0x099782e84a7cee30)]
    [InlineData(2, "abc", 0x973ed17dfbe006d7)]
    [InlineData(3, "message digest", 0xc0189aa4012331f5)]
    [InlineData(4, "abcdefghijklmnopqrstuvwxyz", 0xda133f940b62e516)]
    [InlineData(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 0xe062dfda99413626)]
    [InlineData(6, "12345678901234567890123456789012345678901234567890123456789012345678901234567890", 0x77092dd38803d1fa)]
    public void Wy4Hash64TestVectors(int seed, string value, ulong hash)
    {
        ulong h = Wy4Hash64.ComputeHash(Encoding.ASCII.GetBytes(value), (ulong)seed);
        Assert.Equal(hash, h);
    }

    [Fact]
    public void Wy3Hash64IndexTest()
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

    [Fact]
    public void Wy4Hash64IndexTest()
    {
        ulong[] inputs =
        [
            0UL,
            1UL,
            0x0123456789abcdefUL,
            12808224424451380151UL,
            ulong.MaxValue
        ];

        foreach (ulong input in inputs)
        {
            Span<byte> data = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(data, input);

            Assert.Equal(Wy4Hash64.ComputeHash(data), Wy4Hash64.ComputeIndex(input));
            Assert.Equal(Wy4Hash64.ComputeHash(data, 123), Wy4Hash64.ComputeIndex(input, 123));
        }
    }

    [Fact]
    public unsafe void Wy4Hash64UnsafeMatchesManaged()
    {
        byte[] data = new byte[256];
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)i;

        fixed (byte* ptr = data)
        {
            for (int i = 0; i <= data.Length; i++)
            {
                Assert.Equal(Wy4Hash64.ComputeHash(data.AsSpan(0, i)), Wy4Hash64Unsafe.ComputeHash(ptr, i));
                Assert.Equal(Wy4Hash64.ComputeHash(data.AsSpan(0, i), 123), Wy4Hash64Unsafe.ComputeHash(ptr, i, 123));
            }
        }
    }
}