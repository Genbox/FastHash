using Genbox.FastHash.TestShared;

namespace Genbox.FastHash.Tests;

public class HashTests
{
    private static readonly byte[] _data = "This is a test!!"u8.ToArray();

    public static IEnumerable<object[]> CreateAlgorithms32() => AlgorithmCatalog.Hash32Algorithms
        .Where(static x => x.Hash != null && x.Expected != null)
        .Select(static x => new object[] { x.Name, x.Expected! });

    public static IEnumerable<object[]> CreateAlgorithms64() => AlgorithmCatalog.Hash64Algorithms
        .Where(static x => x.Hash != null && x.Expected != null)
        .Select(static x => new object[] { x.Name, x.Expected! });

    public static IEnumerable<object[]> CreateAlgorithms128() => AlgorithmCatalog.Hash128Algorithms
        .Where(static x => x.Hash != null && x.Expected != null)
        .Select(static x => new object[] { x.Name, x.Expected! });

    public static IEnumerable<object[]> CreateAlgorithmsUnsafe32() => AlgorithmCatalog.Hash32Algorithms
        .Where(static x => x.UnsafeHash != null && x.Expected != null)
        .Select(static x => new object[] { x.Name, x.Expected! });

    public static IEnumerable<object[]> CreateAlgorithmsUnsafe64() => AlgorithmCatalog.Hash64Algorithms
        .Where(static x => x.UnsafeHash != null && x.Expected != null)
        .Select(static x => new object[] { x.Name, x.Expected! });

    public static IEnumerable<object[]> CreateAlgorithmsUnsafe128() => AlgorithmCatalog.Hash128Algorithms
        .Where(static x => x.UnsafeHash != null && x.Expected != null)
        .Select(static x => new object[] { x.Name, x.Expected! });

    [Theory]
    [MemberData(nameof(CreateAlgorithms32))]
    public void Check32(string name, byte[] expected)
    {
        Hash32Algorithm algorithm = AlgorithmCatalog.GetHash32(name);
        Assert.Equal(expected, BitConverter.GetBytes(algorithm.Hash!(_data)));
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms64))]
    public void Check64(string name, byte[] expected)
    {
        Hash64Algorithm algorithm = AlgorithmCatalog.GetHash64(name);
        Assert.Equal(expected, BitConverter.GetBytes(algorithm.Hash!(_data)));
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms128))]
    public void Check128(string name, byte[] expected)
    {
        Hash128Algorithm algorithm = AlgorithmCatalog.GetHash128(name);
        Assert128(expected, algorithm.Hash!(_data));
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe32))]
    public void CheckUnsafe32(string name, byte[] expected)
    {
        Hash32Algorithm algorithm = AlgorithmCatalog.GetHash32(name);
        Assert.Equal(expected, BitConverter.GetBytes(HashUnsafe(algorithm.UnsafeHash!)));
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe64))]
    public void CheckUnsafe64(string name, byte[] expected)
    {
        Hash64Algorithm algorithm = AlgorithmCatalog.GetHash64(name);
        Assert.Equal(expected, BitConverter.GetBytes(HashUnsafe(algorithm.UnsafeHash!)));
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe128))]
    public void CheckUnsafe128(string name, byte[] expected)
    {
        Hash128Algorithm algorithm = AlgorithmCatalog.GetHash128(name);
        Assert128(expected, HashUnsafe(algorithm.UnsafeHash!));
    }

    private static void Assert128(byte[] expected, UInt128 actual)
    {
        UInt128 expectedVal = new UInt128(BitConverter.ToUInt64(expected), BitConverter.ToUInt64(expected, 8));
        Assert.Equal(expectedVal, actual);
    }

    private static unsafe uint HashUnsafe(Hash32Unsafe hash)
    {
        fixed (byte* ptr = _data)
            return hash(ptr, _data.Length);
    }

    private static unsafe ulong HashUnsafe(Hash64Unsafe hash)
    {
        fixed (byte* ptr = _data)
            return hash(ptr, _data.Length);
    }

    private static unsafe UInt128 HashUnsafe(Hash128Unsafe hash)
    {
        fixed (byte* ptr = _data)
            return hash(ptr, _data.Length);
    }
}