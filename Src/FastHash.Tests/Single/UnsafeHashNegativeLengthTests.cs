using Genbox.FastHash.AbslHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.ClHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.HighwayHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Tests.Single;

public class UnsafeHashNegativeLengthTests
{
    [Fact]
    public unsafe void ComputeHashRejectsNegativeLengthBeforeReadingData()
    {
        byte* data = null;

        Assert.Throws<ArgumentOutOfRangeException>(() => AbslHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AbslHash64Unsafe.ComputeHash(data, -1, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHash128Unsafe.ComputeHash(data, -1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHashCrc128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHashCrc128Unsafe.ComputeHash(data, -1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CityHashCrc256Unsafe.ComputeHash(data, -1, null));

        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash64Unsafe.ComputeHash(data, -1, new ulong[ClHashConstants.Random64BitWordsNeeded]));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClHash64Unsafe.ComputeHash(data, -1, (ulong*)null));

        Assert.Throws<ArgumentOutOfRangeException>(() => Djb2Hash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Djb2Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Djb2AltHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Djb2AltHash64Unsafe.ComputeHash(data, -1));

        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarshHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FarmHash128Unsafe.ComputeHash(data, -1, default));

        Assert.Throws<ArgumentOutOfRangeException>(() => Fnv1aHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Fnv1aHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => HighwayHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => HighwayHash64Unsafe.ComputeHash(data, -1, 1, 2, 3, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => HighwayHash64Unsafe.ComputeHash(data, -1, new ulong[4]));
        Assert.Throws<ArgumentOutOfRangeException>(() => MeowHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MeowHash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3Hash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3Hash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3Hash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3Hash128Unsafe.ComputeHash(data, -1, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash64Unsafe.ComputeHash(data, -1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => SipHash64Unsafe.ComputeHash(data, -1, 1, 2, 2, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => SuperFastHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SuperFastHash32Unsafe.ComputeHash(data, -1, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => Wy2Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Wy2Hash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Wy3Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Wy3Hash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Wy4Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Wy4Hash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash32Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash32Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => XxHash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Xx3Hash64Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Xx3Hash64Unsafe.ComputeHash(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Xx3Hash128Unsafe.ComputeHash(data, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Xx3Hash128Unsafe.ComputeHash(data, -1, 1));
    }
}