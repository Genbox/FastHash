#if NET8_0_OR_GREATER
using Genbox.FastHash.ClHash;

namespace Genbox.FastHash.Tests.Single;

public class ClHashTests
{
    private static readonly Vector[] Vectors =
    [
        new(0x0000000000000001UL, 0x0000000000000002UL, [], 0, 0x0000000000000000UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "a"u8.ToArray(), 1, 0xacdafcbac501bd30UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "ab"u8.ToArray(), 2, 0x59b6ffb15b82bb2fUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "abc"u8.ToArray(), 3, 0xf56dc687673624d3UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "1234567"u8.ToArray(), 7, 0xef7c0d4bc4edaeb5UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "12345678"u8.ToArray(), 8, 0x87a001bd8c8dafffUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "123456789"u8.ToArray(), 9, 0x5760dd6ecd63f100UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "abcdefghijklmnop"u8.ToArray(), 16, 0x7ebb571f25d48fcbUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, "The quick brown fox jumps over the lazy dog"u8.ToArray(), 43, 0x0f6a83d8db4a998cUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 64, 0xacdf8cc8a72bfde4UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 100, 0x1f60236eda78ab3aUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 127, 0x7a5aee3ac782c430UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 128, 0x2862c359af8542baUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 1016, 0x2617c19c67e57316UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 1023, 0x3235cc7231b295aeUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 1024, 0x1fe72f990185b827UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 1025, 0x12ac8f7233faec1aUL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 1031, 0x05f81daaea50a081UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 2048, 0x2d8de569eadfdfd3UL),
        new(0x0000000000000001UL, 0x0000000000000002UL, null, 4096, 0x161626acc79338d6UL),

        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, [], 0, 0x0000000000000000UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "a"u8.ToArray(), 1, 0x4ea7e19b3349b1b4UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "ab"u8.ToArray(), 2, 0x7986c3e43cb8ed61UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "abc"u8.ToArray(), 3, 0x48ac6e6f91526149UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "1234567"u8.ToArray(), 7, 0x3e1e6dc170a084e1UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "12345678"u8.ToArray(), 8, 0x02ec9d5ed2b10bb4UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "123456789"u8.ToArray(), 9, 0xb0f75433e605c3bdUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "abcdefghijklmnop"u8.ToArray(), 16, 0x9eb5bb194ebe739bUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, "The quick brown fox jumps over the lazy dog"u8.ToArray(), 43, 0xf079e453f4d1d1f7UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 64, 0x769b8d02511a2d8dUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 100, 0x8b6fb22f92dea08cUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 127, 0xa57195473dc2581aUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 128, 0x3a00a31f270bbdedUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 1016, 0x5a120e562baa8754UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 1023, 0xeee727c8f7378128UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 1024, 0xe9596a9495802c1eUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 1025, 0xa8ed3395c92b425eUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 1031, 0xa23ceab428aac1a5UL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 2048, 0x5121c873b32faedeUL),
        new(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL, null, 4096, 0xfe60f0243de4f18eUL)
    ];

    [Fact]
    public unsafe void TestVectors()
    {
        if (!ClHash64.IsSupported)
            return;

        byte[] pattern = CreatePattern(4096);

        foreach (Vector vector in Vectors)
        {
            byte[] data = vector.Data ?? pattern.AsSpan(0, vector.Length).ToArray();
            ulong[] key = ClHashShared.CreateKey(vector.Seed1, vector.Seed2);

            Assert.Equal(vector.Expected, ClHash64.ComputeHash(data, vector.Seed1, vector.Seed2));
            Assert.Equal(vector.Expected, ClHash64.ComputeHash(data, key));

            byte[] buffer = data.Length == 0 ? new byte[1] : data;
            fixed (byte* ptr = buffer)
            {
                Assert.Equal(vector.Expected, ClHash64Unsafe.ComputeHash(ptr, vector.Length, vector.Seed1, vector.Seed2));
                Assert.Equal(vector.Expected, ClHash64Unsafe.ComputeHash(ptr, vector.Length, key));
            }
        }
    }

    [Fact]
    public void ReusedKeyIsDeterministic()
    {
        if (!ClHash64.IsSupported)
            return;

        byte[] data = "my dog"u8.ToArray();
        ulong[] key1 = ClHashShared.CreateKey(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL);
        ulong[] key2 = ClHashShared.CreateKey(0x23a23cf5033c3c81UL, 0xb3816f6a2c68e530UL);

        Assert.Equal(key1, key2);
        Assert.Equal(ClHash64.ComputeHash(data, key1), ClHash64.ComputeHash(data, key2));
        Assert.NotEqual(ClHash64.ComputeHash(data, key1), ClHash64.ComputeHash("my cat"u8, key1));
    }

    [Fact]
    public void CollisionRegression()
    {
        if (!ClHash64.IsSupported)
            return;

        ulong[] key = CreateByteOffsetKey(0x63);

        for (int i = 1; i < 10; i++)
        {
            for (int j = 1; j <= sizeof(ulong); j++)
            {
                int length = (i * 1024) + j;
                byte[] data = new byte[length];

                for (int k = 0; k < data.Length; k++)
                    data[k] = unchecked((byte)k);

                ulong hash1 = ClHash64.ComputeHash(data, key);
                data[^1] = unchecked((byte)(data[^1] + 1));
                ulong hash2 = ClHash64.ComputeHash(data, key);

                Assert.NotEqual(hash1, hash2);
            }
        }
    }

    [Fact]
    public void FlipBitRegression()
    {
        if (!ClHash64.IsSupported)
            return;

        ulong[] key = CreateByteOffsetKey(1, negateIndex: true);

        for (int bit = 0; bit < 64; bit++)
        {
            for (int length = (bit + 8) / 8; length <= sizeof(ulong); length++)
            {
                byte[] data = new byte[sizeof(ulong)];
                ulong original = ClHash64.ComputeHash(data.AsSpan(0, length), key);
                data[bit >> 3] ^= (byte)(1 << (bit & 7));
                ulong flip = ClHash64.ComputeHash(data.AsSpan(0, length), key);
                data[bit >> 3] ^= (byte)(1 << (bit & 7));
                ulong back = ClHash64.ComputeHash(data.AsSpan(0, length), key);

                Assert.NotEqual(original, flip);
                Assert.Equal(original, back);
            }
        }
    }

    [Fact]
    public void AvalancheSmokeTest()
    {
        if (!ClHash64.IsSupported)
            return;

        ulong[] key = CreateQuadraticByteKey();

        for (int length = 1; length < 16; length++)
        {
            for (int value = 0; value < 256; value++)
            {
                byte[] data1 = Enumerable.Repeat((byte)value, length).ToArray();
                byte[] data2 = Enumerable.Repeat(unchecked((byte)(value + 35)), length).ToArray();
                ulong original1 = ClHash64.ComputeHash(data1, key);
                ulong original2 = ClHash64.ComputeHash(data2, key);

                for (int bit = 0; bit < 8 * length; bit++)
                {
                    FlipBit(data1, bit);
                    ulong hash1 = ClHash64.ComputeHash(data1, key);
                    FlipBit(data1, bit);

                    FlipBit(data2, bit);
                    ulong hash2 = ClHash64.ComputeHash(data2, key);
                    FlipBit(data2, bit);

                    Assert.NotEqual(original1, hash1);
                    Assert.NotEqual(original2, hash2);

                    if (length <= sizeof(ulong))
                        Assert.Equal(original1 ^ hash1, original2 ^ hash2);
                }
            }
        }
    }

    private static byte[] CreatePattern(int length)
    {
        byte[] data = new byte[length];

        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)((i * 0x9E + 0x37) & 0xFF);

        return data;
    }

    private static ulong[] CreateByteOffsetKey(byte offset, bool negateIndex = false)
    {
        byte[] keyBytes = new byte[ClHashConstants.RandomBytesNeeded];

        for (int i = 0; i < keyBytes.Length; i++)
            keyBytes[i] = unchecked((byte)(negateIndex ? offset - i : i + offset));

        return BytesToULongs(keyBytes);
    }

    private static ulong[] CreateQuadraticByteKey()
    {
        byte[] keyBytes = new byte[ClHashConstants.RandomBytesNeeded];

        for (int i = 0; i < keyBytes.Length; i++)
            keyBytes[i] = unchecked((byte)(i + 1 - (i * i)));

        return BytesToULongs(keyBytes);
    }

    private static ulong[] BytesToULongs(byte[] bytes)
    {
        ulong[] result = new ulong[ClHashConstants.Random64BitWordsNeeded];
        Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
        return result;
    }

    private static void FlipBit(byte[] data, int bit) => data[bit >> 3] ^= (byte)(1 << (bit & 7));

    private readonly record struct Vector(ulong Seed1, ulong Seed2, byte[]? Data, int Length, ulong Expected);
}
#endif