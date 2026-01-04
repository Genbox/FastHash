using System.Buffers.Binary;
using Genbox.FastHash.FoldHash;

namespace Genbox.FastHash.Tests.Single;

public class FoldHashTests
{
    private static readonly byte[] QuickFox = "The quick brown fox jumps over the lazy dog"u8.ToArray();
    private static readonly byte[] Hello = "hello"u8.ToArray();
    private static readonly byte[] HelloWorld = "hello world"u8.ToArray();
    private static readonly byte[] Zeros1000 = new byte[1000];

    private readonly record struct Vector(byte[] Data, ulong Seed, ulong Expected);

    private static readonly Vector[] FastVectors =
    [
        new Vector([], 0, 0x01ee54dc4d5e9d2dUL),
        new Vector("a"u8.ToArray(), 0, 0x4d9532b828b2c6e4UL),
        new Vector(Hello, 0, 0x26a30a404aaedb54UL),
        new Vector(HelloWorld, 0, 0xee1e9c9877346398UL),
        new Vector(QuickFox, 0, 0x289ea8116f1910a4UL),
        new Vector(Zeros1000, 0, 0xead7153b09276730UL),
        new Vector([], 123, 0x13628f54f37b47c2UL),
        new Vector("a"u8.ToArray(), 123, 0x3297610983f628bcUL),
        new Vector(Hello, 123, 0x2617e75e24d0373bUL),
        new Vector(HelloWorld, 123, 0xf0682e03604cf4f9UL),
        new Vector(QuickFox, 123, 0x1a8aa0e976f31668UL),
        new Vector(Zeros1000, 123, 0xf59c4a9a160d63deUL),
    ];

    private static readonly Vector[] QualityVectors =
    [
        new Vector([], 0, 0xcfbc9eaebd51464bUL),
        new Vector("a"u8.ToArray(), 0, 0x3199587abf734b40UL),
        new Vector(Hello, 0, 0xbea9589fe22f3e57UL),
        new Vector(HelloWorld, 0, 0x2b2c8698e6a24507UL),
        new Vector(QuickFox, 0, 0x15a7cc070c677daeUL),
        new Vector(Zeros1000, 0, 0xeb813d80b8365dfeUL),
        new Vector([], 123, 0x30ff99109d48bc70UL),
        new Vector("a"u8.ToArray(), 123, 0x7aac7da8ece41cebUL),
        new Vector(Hello, 123, 0x7a407c76671b9e93UL),
        new Vector(HelloWorld, 123, 0xbb42d001dbbb69d1UL),
        new Vector(QuickFox, 123, 0x98d4cd66a31b2837UL),
        new Vector(Zeros1000, 123, 0x7f93a67e18528e30UL),
    ];

    [Fact]
    public void FastVectorsTest()
    {
        foreach (Vector vector in FastVectors)
            Assert.Equal(vector.Expected, FoldHash64.ComputeHash(vector.Data, vector.Seed));
    }

    [Fact]
    public void QualityVectorsTest()
    {
        foreach (Vector vector in QualityVectors)
            Assert.Equal(vector.Expected, FoldHashQuality64.ComputeHash(vector.Data, vector.Seed));
    }

    [Fact]
    public void FastComputeIndexMatchesByteHash()
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

            Assert.Equal(FoldHash64.ComputeHash(data, 0), FoldHash64.ComputeIndex(input, 0));
            Assert.Equal(FoldHash64.ComputeHash(data, 123), FoldHash64.ComputeIndex(input, 123));
        }
    }

    [Fact]
    public void QualityComputeIndexMatchesByteHash()
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

            Assert.Equal(FoldHashQuality64.ComputeHash(data, 0), FoldHashQuality64.ComputeIndex(input, 0));
            Assert.Equal(FoldHashQuality64.ComputeHash(data, 123), FoldHashQuality64.ComputeIndex(input, 123));
        }
    }
}
