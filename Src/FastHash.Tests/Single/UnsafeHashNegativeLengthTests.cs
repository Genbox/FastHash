namespace Genbox.FastHash.Tests.Single;

public class UnsafeHashNegativeLengthTests
{
    [Fact]
    public unsafe void ComputeHashRejectsNegativeLengthBeforeReadingData()
    {
        byte* data = null;

        Assert.Throws<ArgumentOutOfRangeException>(() => AbslHash.AbslHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AbslHash.AbslHash64Unsafe.ComputeHash(data, -1, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHash128Unsafe.ComputeHash(data, -1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHashCrc128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHashCrc128Unsafe.ComputeHash(data, -1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash.CityHashCrc256Unsafe.ComputeHash(data, -1, null));

        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash.ClHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash.ClHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash.ClHash64Unsafe.ComputeHash(data, -1, new ulong[ClHash.ClHashConstants.Random64BitWordsNeeded]));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash.ClHash64Unsafe.ComputeHash(data, -1, (ulong*)null));

        Assert.Throws<ArgumentOutOfRangeException>(() => DjbHash.Djb2Hash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => DjbHash.Djb2Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => DjbHash.Djb2AltHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => DjbHash.Djb2AltHash64Unsafe.ComputeHash(data, -1));

        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash.FarshHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash.FarshHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash.FarshHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash.FarshHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash.FarmHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash.FarmHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash.FarmHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash.FarmHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash.FarmHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash.FarmHash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash.FarmHash128Unsafe.ComputeHash(data, -1, default));

        Assert.Throws<ArgumentOutOfRangeException>(() => FnvHash.Fnv1aHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FnvHash.Fnv1aHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => HighwayHash.HighwayHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => HighwayHash.HighwayHash64Unsafe.ComputeHash(data, -1, 1, 2, 3, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => HighwayHash.HighwayHash64Unsafe.ComputeHash(data, -1, new ulong[4]));
        Assert.Throws<ArgumentOutOfRangeException>(() => MeowHash.MeowHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MeowHash.MeowHash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MurmurHash.Murmur3Hash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MurmurHash.Murmur3Hash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MurmurHash.Murmur3Hash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MurmurHash.Murmur3Hash128Unsafe.ComputeHash(data, -1, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash.SipHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash.SipHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash.SipHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash.SipHash64Unsafe.ComputeHash(data, -1, 1, 2, 2, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => SuperFastHash.SuperFastHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SuperFastHash.SuperFastHash32Unsafe.ComputeHash(data, -1, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => WyHash.Wy3Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => WyHash.Wy3Hash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => WyHash.Wy4Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => WyHash.Wy4Hash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.XxHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.XxHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.XxHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.XxHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.Xx3Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.Xx3Hash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.Xx3Hash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash.Xx3Hash128Unsafe.ComputeHash(data, -1, 1));
    }
}