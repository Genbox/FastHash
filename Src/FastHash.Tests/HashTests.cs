using Genbox.FastHash.TestShared;

namespace Genbox.FastHash.Tests;

public class HashTests
{
    private static readonly byte[] _data = "This is a test!!"u8.ToArray();

    public static IEnumerable<object[]> CreateAlgorithms32() => AlgorithmCatalog.Hash32Algorithms
                                                                                .Where(static x => x.Hash != null && x.Expected != null)
                                                                                .Select(static x => new object[] { x.Name });

    public static IEnumerable<object[]> CreateAlgorithms64() => AlgorithmCatalog.Hash64Algorithms
                                                                                .Where(static x => x.Hash != null && x.Expected != null)
                                                                                .Select(static x => new object[] { x.Name });

    public static IEnumerable<object[]> CreateAlgorithms128() => AlgorithmCatalog.Hash128Algorithms
                                                                                 .Where(static x => x.Hash != null && x.Expected != null)
                                                                                 .Select(static x => new object[] { x.Name });

    [Theory]
    [MemberData(nameof(CreateAlgorithms32))]
    public void Check32(string name)
    {
        Hash32Algorithm algorithm = AlgorithmCatalog.GetHash32(name);
        Assert.SkipWhen(!algorithm.IsSupported, $"{name} is unavailable on this CPU.");
        Assert.Equal(algorithm.Expected!.Value, algorithm.Hash!(_data));
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms64))]
    public void Check64(string name)
    {
        Hash64Algorithm algorithm = AlgorithmCatalog.GetHash64(name);
        Assert.SkipWhen(!algorithm.IsSupported, $"{name} is unavailable on this CPU.");
        Assert.Equal(algorithm.Expected!.Value, algorithm.Hash!(_data));
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms128))]
    public void Check128(string name)
    {
        Hash128Algorithm algorithm = AlgorithmCatalog.GetHash128(name);
        Assert.SkipWhen(!algorithm.IsSupported, $"{name} is unavailable on this CPU.");
        Assert.Equal(algorithm.Expected!.Value, algorithm.Hash!(_data));
    }

}