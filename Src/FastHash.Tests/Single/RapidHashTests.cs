using System.Buffers.Binary;
using Genbox.FastHash.RapidHash;

namespace Genbox.FastHash.Tests.Single;

public class RapidHashTests
{
    private static readonly byte[] QuickFox = "The quick brown fox jumps over the lazy dog"u8.ToArray();
    private static readonly byte[] Hello = "hello"u8.ToArray();
    private static readonly byte[] HelloWorld = "hello world"u8.ToArray();
    private static readonly byte[] Zeros1000 = new byte[1000];

    private readonly record struct Vector(byte[] Data, ulong Seed, ulong Expected);

    private static readonly Vector[] Vectors =
    [
        new Vector([], 0, 0x0338dc4be2cecdaeUL),
        new Vector("a"u8.ToArray(), 0, 0x599f47df33a2e1ebUL),
        new Vector(Hello, 0, 0x2e2d7651b45f7946UL),
        new Vector(HelloWorld, 0, 0x2f27cb27d5240940UL),
        new Vector(QuickFox, 0, 0x91722dc8d52a3f7bUL),
        new Vector(Zeros1000, 0, 0x1a2f2e1ba3bc17dcUL),
        new Vector([], 123, 0xdff2041cce4be562UL),
        new Vector("a"u8.ToArray(), 123, 0x5512cb31e97c34a7UL),
        new Vector(Hello, 123, 0xf31c541b486d28aeUL),
        new Vector(HelloWorld, 123, 0x686b8e902b13032eUL),
        new Vector(QuickFox, 123, 0x0896531731a6844eUL),
        new Vector(Zeros1000, 123, 0xd1f8a452e032c247UL),
    ];

    [Fact]
    public void TestVectors()
    {
        foreach (Vector vector in Vectors)
            Assert.Equal(vector.Expected, RapidHash64.ComputeHash(vector.Data, vector.Seed));
    }

    [Fact]
    public void ComputeIndexMatchesByteHash()
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

            Assert.Equal(RapidHash64.ComputeHash(data, 0), RapidHash64.ComputeIndex(input, 0));
            Assert.Equal(RapidHash64.ComputeHash(data, 123), RapidHash64.ComputeIndex(input, 123));
        }
    }

    [Fact]
    public void MicroMatchesRapidForShortInputs()
    {
        byte[][] inputs =
        [
            [],
            "a"u8.ToArray(),
            Hello,
            HelloWorld,
            QuickFox
        ];

        foreach (byte[] data in inputs)
        {
            Assert.Equal(RapidHash64.ComputeHash(data, 0), RapidHashMicro64.ComputeHash(data, 0));
            Assert.Equal(RapidHash64.ComputeHash(data, 123), RapidHashMicro64.ComputeHash(data, 123));
        }
    }

    [Fact]
    public void NanoMatchesRapidForShortInputs()
    {
        byte[][] inputs =
        [
            [],
            "a"u8.ToArray(),
            Hello,
            HelloWorld
        ];

        foreach (byte[] data in inputs)
        {
            Assert.Equal(RapidHash64.ComputeHash(data, 0), RapidHashNano64.ComputeHash(data, 0));
            Assert.Equal(RapidHash64.ComputeHash(data, 123), RapidHashNano64.ComputeHash(data, 123));
        }
    }

    [Fact]
    public void MicroAndNanoRespectSeedOnLongInput()
    {
        ulong micro0 = RapidHashMicro64.ComputeHash(Zeros1000, 0);
        ulong micro1 = RapidHashMicro64.ComputeHash(Zeros1000, 123);
        ulong nano0 = RapidHashNano64.ComputeHash(Zeros1000, 0);
        ulong nano1 = RapidHashNano64.ComputeHash(Zeros1000, 123);

        Assert.NotEqual(micro0, micro1);
        Assert.NotEqual(nano0, nano1);
    }
}