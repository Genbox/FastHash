using System.Buffers.Binary;
using System.Text;
using Genbox.FastHash.WyHash;

namespace Genbox.FastHash.Tests.Single;

public class WyHashTests
{
#if !WYHASH_CONDOM

    // wyhash 3.0 vectors from old_versions/wyhash_final2.h.
    [Theory]
    [InlineData(0, "", 0x42bc986dc5eec4d3)]
    [InlineData(1, "a", 0x84508dc903c31551)]
    [InlineData(2, "abc", 0x0bc54887cfc9ecb1)]
    [InlineData(3, "message digest", 0xadc146444841c430)]
    [InlineData(4, "abcdefghijklmnopqrstuvwxyz", 0x9a64e42e897195b9)]
    [InlineData(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 0x9199383239c32554)]
    [InlineData(6, "12345678901234567890123456789012345678901234567890123456789012345678901234567890", 0x7c1ccf6bba30f5a5)]
    public void Wy2Hash64TestVectors(int seed, string value, ulong hash)
    {
        ulong h = Wy2Hash64.ComputeHash(Encoding.ASCII.GetBytes(value), (ulong)seed);
        Assert.Equal(hash, h);
    }

    // Generated from wyhash final 4.3's wyhash.h using its default secret.
    [Theory]
    [InlineData(0, "", 0x93228a4de0eec5a2)]
    [InlineData(1, "a", 0xc5bac3db178713c4)]
    [InlineData(2, "abc", 0xa97f2f7b1d9b3314)]
    [InlineData(3, "message digest", 0x786d1f1df3801df4)]
    [InlineData(4, "abcdefghijklmnopqrstuvwxyz", 0xdca5a8138ad37c87)]
    [InlineData(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 0xb9e734f117cfaf70)]
    [InlineData(6, "12345678901234567890123456789012345678901234567890123456789012345678901234567890", 0x6cc5eab49a92d617)]
    public void Wy4Hash64TestVectors(int seed, string value, ulong hash)
    {
        ulong h = Wy4Hash64.ComputeHash(Encoding.ASCII.GetBytes(value), (ulong)seed);
        Assert.Equal(hash, h);
    }

    [Theory]
    [InlineData(0, 0xf78a624e3f5ae9d6)]
    [InlineData(1, 0x1403f8b7835414c8)]
    [InlineData(3, 0xc4c72ea3290f5f3f)]
    [InlineData(4, 0x5c3ee7e4e75b488a)]
    [InlineData(8, 0x216abd9dd3a82231)]
    [InlineData(9, 0x31dda67e6c01b6e4)]
    [InlineData(16, 0x0294a3a1a8d0a6f3)]
    [InlineData(17, 0x246f84cbe179954e)]
    [InlineData(47, 0xe3ee8af2d778a44f)]
    [InlineData(48, 0xfc8bbf90ec9f15a5)]
    [InlineData(49, 0xc9b39ca6809d7305)]
    [InlineData(95, 0xa5357027dd39f0e2)]
    [InlineData(96, 0x827837bf73ab0484)]
    [InlineData(97, 0xcc62b21ab0dd827b)]
    [InlineData(255, 0x71b66f77e4a7a006)]
    [InlineData(256, 0x218464befa2a3270)]
    [InlineData(257, 0x08216378a0488085)]
    public void Wy2Hash64BoundaryVectors(int length, ulong hash)
    {
        byte[] data = CreateTestData(length);
        Assert.Equal(hash, Wy2Hash64.ComputeHash(data, 123));
    }

    [Theory]
    [InlineData(0, 0xcb611538dfe036ff)]
    [InlineData(1, 0xbc91649baa9ee1a6)]
    [InlineData(3, 0x6c49661ea52c54df)]
    [InlineData(4, 0x4145b911d54cae51)]
    [InlineData(8, 0x781d479043135bbf)]
    [InlineData(9, 0x83f7d1897b88df1c)]
    [InlineData(16, 0x8c56a29621f298f2)]
    [InlineData(17, 0xf5a18556c80d7934)]
    [InlineData(47, 0xf24158ab43363af0)]
    [InlineData(48, 0x74b92e7372a2f752)]
    [InlineData(49, 0xe807cf993ef9c988)]
    [InlineData(95, 0x0f7f3f4b1095ffe0)]
    [InlineData(96, 0xd8d45a1e69b6c359)]
    [InlineData(97, 0x1ca5f5a18791c638)]
    [InlineData(255, 0x0f28aaf7a8dee042)]
    [InlineData(256, 0x39bef4d84f59cf8d)]
    [InlineData(257, 0x9f7f1c66b9465fcd)]
    public void Wy4Hash64BoundaryVectors(int length, ulong hash)
    {
        byte[] data = CreateTestData(length);
        Assert.Equal(hash, Wy4Hash64.ComputeHash(data, 123));
    }

    [Theory]
    [InlineData(0, 0x00000000000001e8)]
    [InlineData(3, 0x0000000000007a00)]
    [InlineData(4, 0x00364844d5d8edb8)]
    [InlineData(8, 0x00d34163fa8759d8)]
    [InlineData(9, 0x203e6b4972f0d9bf)]
    [InlineData(16, 0x2ed87bcc1c57e97a)]
    [InlineData(48, 0x1f4742a4b9460fce)]
    [InlineData(97, 0x65f43dd1c27cadf1)]
    public void Wy2Hash64CustomSecretVectors(int length, ulong hash) => Assert.Equal(hash, Wy2Hash64.ComputeHash(CreateTestData(length), 123, [1, 2, 3, 4]));

    [Theory]
    [InlineData(0, 0x000000000000023e)]
    [InlineData(3, 0x0000000000011e04)]
    [InlineData(4, 0x04c7c13f21694f8e)]
    [InlineData(8, 0xfc2bb191d1e53717)]
    [InlineData(9, 0x125986e0754e4106)]
    [InlineData(16, 0x33884072b5961b23)]
    [InlineData(48, 0xd7626a4a45fbbb4e)]
    [InlineData(97, 0x01f08aff0d34880a)]
    public void Wy4Hash64CustomSecretVectors(int length, ulong hash) => Assert.Equal(hash, Wy4Hash64.ComputeHash(CreateTestData(length), 123, [1, 2, 3, 4]));
#endif

    [Fact]
    public void CustomSecretLengthsAreValidated()
    {
        Assert.Throws<ArgumentException>(() => Wy2Hash64.ComputeHash([], []));
        Assert.Equal(Wy2Hash64.ComputeHash([], [1, 2, 3, 4]), Wy2Hash64.ComputeHash([], [1, 2, 3, 4, 5]));
        Assert.Throws<ArgumentException>(() => Wy4Hash64.ComputeHash([], []));
        Assert.Equal(Wy4Hash64.ComputeHash([], [1, 2, 3, 4]), Wy4Hash64.ComputeHash([], [1, 2, 3, 4, 5]));
    }

#if WYHASH_CONDOM
    [Fact]
    public unsafe void WyHashCondom2Vectors()
    {
        byte[] pattern = CreateTestData(97);

        Assert.Equal(0xE6C763C9230F5746UL, Wy2Hash64.ComputeHash([]));
        Assert.Equal(0xE81BB997CC2CC450UL, Wy2Hash64.ComputeHash("abc"u8, 2));
        Assert.Equal(0xCE608CE57D9F025EUL, Wy2Hash64.ComputeHash(pattern, 123));
        Assert.Equal(0x4C91B2FDB699FF5FUL, Wy4Hash64.ComputeHash([]));
        Assert.Equal(0xBA31EE45A25CB04FUL, Wy4Hash64.ComputeHash("abc"u8, 2));
        Assert.Equal(0x919140C75D7ADBCEUL, Wy4Hash64.ComputeHash(pattern, 123));

        fixed (byte* ptr = pattern)
        {
            Assert.Equal(0xCE608CE57D9F025EUL, Wy2Hash64Unsafe.ComputeHash(ptr, pattern.Length, 123));
            Assert.Equal(0x919140C75D7ADBCEUL, Wy4Hash64Unsafe.ComputeHash(ptr, pattern.Length, 123));
        }
    }
#endif

    [Fact]
    public void Wy2Hash64IndexTest()
    {
        ulong val = 1ul;

        for (int i = 1; i <= 64; i++)
        {
            ulong h1 = Wy2Hash64.ComputeHash(BitConverter.GetBytes(val));
            ulong h2 = Wy2Hash64.ComputeIndex(val);
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
        Span<byte> data = stackalloc byte[8];

        foreach (ulong input in inputs)
        {
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
            data[i] = unchecked((byte)i);

        fixed (byte* ptr = data)
        {
            for (int i = 0; i <= data.Length; i++)
            {
                Assert.Equal(Wy4Hash64.ComputeHash(data.AsSpan(0, i)), Wy4Hash64Unsafe.ComputeHash(ptr, i));
                Assert.Equal(Wy4Hash64.ComputeHash(data.AsSpan(0, i), 123), Wy4Hash64Unsafe.ComputeHash(ptr, i, 123));
            }
        }
    }

    [Fact]
    public unsafe void Wy2Hash64UnsafeMatchesManaged()
    {
        byte[] data = new byte[257];
        for (int i = 0; i < data.Length; i++)
            data[i] = unchecked((byte)i);

        fixed (byte* ptr = data)
        {
            for (int i = 0; i <= data.Length; i++)
            {
                Assert.Equal(Wy2Hash64.ComputeHash(data.AsSpan(0, i)), Wy2Hash64Unsafe.ComputeHash(ptr, i));
                Assert.Equal(Wy2Hash64.ComputeHash(data.AsSpan(0, i), 123), Wy2Hash64Unsafe.ComputeHash(ptr, i, 123));
            }
        }
    }

    private static byte[] CreateTestData(int length)
    {
        byte[] data = new byte[length];
        for (int i = 0; i < data.Length; i++)
            data[i] = unchecked((byte)i);

        return data;
    }
}