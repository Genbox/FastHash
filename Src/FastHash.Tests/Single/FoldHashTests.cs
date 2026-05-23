using Genbox.FastHash.FoldHash;

namespace Genbox.FastHash.Tests.Single;

public class FoldHashTests
{
    private static readonly byte[] QuickFox = "The quick brown fox jumps over the lazy dog"u8.ToArray();
    private static readonly byte[] Hello = "hello"u8.ToArray();
    private static readonly byte[] HelloWorld = "hello world"u8.ToArray();
    private static readonly byte[] Zeros1000 = new byte[1000];

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
        new Vector(Zeros1000, 123, 0xf59c4a9a160d63deUL)
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
        new Vector(Zeros1000, 123, 0x7f93a67e18528e30UL)
    ];

    private static readonly IndexVector[] IndexVectors =
    [
        new IndexVector(0UL, 0, 0xAD05CD10D0315F5DUL, 0xB7D74184AC59C943UL),
        new IndexVector(1UL, 0, 0x678267DBB38A1AB7UL, 0x86FC857CC78FD78EUL),
        new IndexVector(0x0123456789abcdefUL, 0, 0x393666F2108FE4FEUL, 0x9597582280314373UL),
        new IndexVector(12808224424451380151UL, 0, 0x361781E54EC25CC8UL, 0x6897C0253374EC78UL),
        new IndexVector(ulong.MaxValue, 0, 0x0EE1CE90DFAA1E5DUL, 0xEEC2462AF43D95F7UL),
        new IndexVector(0UL, 123, 0xC2084AE858EB7C17UL, 0x3073861F9AD95497UL),
        new IndexVector(1UL, 123, 0x778FE0233B5039FDUL, 0xAA3072DE46B93FD8UL),
        new IndexVector(0x0123456789abcdefUL, 123, 0xFBE66D6A3310FE81UL, 0xE363F9E9B247A26FUL),
        new IndexVector(12808224424451380151UL, 123, 0x2C0B0B73F271A6ADUL, 0x6D1A897F6D4C975AUL),
        new IndexVector(ulong.MaxValue, 123, 0x251242C846CC7B18UL, 0x67F7EA980D0B63B8UL)
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
    public void FastComputeIndexVectors()
    {
        foreach (IndexVector vector in IndexVectors)
            Assert.Equal(vector.FastExpected, FoldHash64.ComputeIndex(vector.Input, vector.Seed));
    }

    [Fact]
    public void QualityComputeIndexVectors()
    {
        foreach (IndexVector vector in IndexVectors)
            Assert.Equal(vector.QualityExpected, FoldHashQuality64.ComputeIndex(vector.Input, vector.Seed));
    }

    [Fact]
    public void CustomSharedSeedMustContainSixValues()
    {
        ulong[] sharedSeed = new ulong[5];

        Assert.Throws<ArgumentException>(() => FoldHash64.ComputeHash(Hello, sharedSeed: sharedSeed));
        Assert.Throws<ArgumentException>(() => FoldHashQuality64.ComputeHash(Hello, sharedSeed: sharedSeed));
    }

    private readonly record struct Vector(byte[] Data, ulong Seed, ulong Expected);
    private readonly record struct IndexVector(ulong Input, ulong Seed, ulong FastExpected, ulong QualityExpected);
}