using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.FoldHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.HighwayHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.PolymurHash;
using Genbox.FastHash.RapidHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.T1haHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;
using Xunit.Sdk;

namespace Genbox.FastHash.Tests;

[SuppressMessage("Design", "CA1034:Nested types should not be visible")]
public class IndexTests
{
    [Theory]
    [MemberData(nameof(Create32))]
    public void Check32(Tuple32 c)
    {
        byte[] valueBytes = new byte[4];

        uint[] values = [0u, uint.MaxValue, 0x5555_5555u, 0xAAAA_AAAAu, 0x0123_4567u, 0xFEDC_BA98u];

        foreach (uint value in values)
            AssertIndex(value);

        for (int i = 0; i < 32; i++)
            AssertIndex(1u << i);

        void AssertIndex(uint value)
        {
            uint h1 = c.Index(value);

            BinaryPrimitives.WriteUInt32LittleEndian(valueBytes, value);
            uint h2 = c.Hash(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    [Theory]
    [MemberData(nameof(Create64))]
    public void Check64(Tuple64 c)
    {
        byte[] valueBytes = new byte[8];

        ulong[] values = [0UL, ulong.MaxValue, 0x5555_5555_5555_5555UL, 0xAAAA_AAAA_AAAA_AAAAUL, 0x0123_4567_89AB_CDEFUL, 0xFEDC_BA98_7654_3210UL];

        foreach (ulong value in values)
            AssertIndex(value);

        for (int i = 0; i < 64; i++)
            AssertIndex(1UL << i);

        void AssertIndex(ulong value)
        {
            ulong h1 = c.Index(value);

            BinaryPrimitives.WriteUInt64LittleEndian(valueBytes, value);
            ulong h2 = c.Hash(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    [Theory]
    [MemberData(nameof(Create128))]
    public void Check128(Tuple128 c)
    {
        byte[] valueBytes = new byte[8];

        ulong[] values = [0UL, ulong.MaxValue, 0x5555_5555_5555_5555UL, 0xAAAA_AAAA_AAAA_AAAAUL, 0x0123_4567_89AB_CDEFUL, 0xFEDC_BA98_7654_3210UL];

        foreach (ulong value in values)
            AssertIndex(value);

        for (int i = 0; i < 64; i++)
            AssertIndex(1UL << i);

        void AssertIndex(ulong value)
        {
            UInt128 h1 = c.Index(value);

            BinaryPrimitives.WriteUInt64LittleEndian(valueBytes, value);
            UInt128 h2 = c.Hash(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    public static TheoryData<Tuple32> Create32() =>
    [
        new Tuple32(nameof(CityHash32), CityHash32.ComputeIndex, CityHash32.ComputeHash),
        new Tuple32(nameof(Djb2AltHash32), Djb2AltHash32.ComputeIndex, Djb2AltHash32.ComputeHash),
        new Tuple32(nameof(Djb2Hash32), Djb2Hash32.ComputeIndex, Djb2Hash32.ComputeHash),
        new Tuple32(nameof(FarmHash32), FarmHash32.ComputeIndex, FarmHash32.ComputeHash),
        new Tuple32(nameof(Fnv1aHash32), Fnv1aHash32.ComputeIndex, Fnv1aHash32.ComputeHash),
        new Tuple32(nameof(Gx2Hash32), Gx2Hash32.ComputeIndex, Gx2Hash32.ComputeHash),
        new Tuple32(nameof(MarvinHash32), MarvinHash32.ComputeIndex, MarvinHash32.ComputeHash),
        new Tuple32(nameof(Murmur3Hash32), Murmur3Hash32.ComputeIndex, Murmur3Hash32.ComputeHash),
        new Tuple32(nameof(SuperFastHash32), SuperFastHash32.ComputeIndex, SuperFastHash32.ComputeHash),
        new Tuple32(nameof(XxHash32), XxHash32.ComputeIndex, XxHash32.ComputeHash)
    ];

    public static TheoryData<Tuple64> Create64() =>
    [
        new Tuple64(nameof(AesniHash64), AesniHash64.ComputeIndex, AesniHash64.ComputeHash),
        new Tuple64(nameof(CityHash64), CityHash64.ComputeIndex, CityHash64.ComputeHash),
        new Tuple64(nameof(Djb2AltHash64), Djb2AltHash64.ComputeIndex, Djb2AltHash64.ComputeHash),
        new Tuple64(nameof(Djb2Hash64), Djb2Hash64.ComputeIndex, Djb2Hash64.ComputeHash),
        new Tuple64(nameof(FarshHash64), FarshHash64.ComputeIndex, FarshHash64.ComputeHash),
        new Tuple64(nameof(FoldHash64), FoldHash64.ComputeIndex, FoldHash64.ComputeHash),
        new Tuple64(nameof(FoldHashQuality64), FoldHashQuality64.ComputeIndex, FoldHashQuality64.ComputeHash),
        new Tuple64(nameof(FarmHash64), FarmHash64.ComputeIndex, FarmHash64.ComputeHash),
        new Tuple64(nameof(Fnv1aHash64), Fnv1aHash64.ComputeIndex, Fnv1aHash64.ComputeHash),
        new Tuple64(nameof(Gx2Hash64), Gx2Hash64.ComputeIndex, Gx2Hash64.ComputeHash),
        new Tuple64(nameof(HighwayHash64), HighwayHash64.ComputeIndex, HighwayHash64.ComputeHash),
        new Tuple64(nameof(MarvinHash64), MarvinHash64.ComputeIndex, MarvinHash64.ComputeHash),
        new Tuple64(nameof(MeowHash64), MeowHash64.ComputeIndex, MeowHash64.ComputeHash),
        new Tuple64(nameof(Polymur2Hash64), Polymur2Hash64.ComputeIndex, Polymur2Hash64.ComputeHash),
        new Tuple64(nameof(Rapid3Hash64), Rapid3Hash64.ComputeIndex, Rapid3Hash64.ComputeHash),
        new Tuple64(nameof(Rapid3HashMicro64), Rapid3HashMicro64.ComputeIndex, Rapid3HashMicro64.ComputeHash),
        new Tuple64(nameof(Rapid3HashNano64), Rapid3HashNano64.ComputeIndex, Rapid3HashNano64.ComputeHash),
        new Tuple64(nameof(SipHash64), SipHash64.ComputeIndex, SipHash64.ComputeHash),
        new Tuple64(nameof(T1ha2Hash64), T1ha2Hash64.ComputeIndex, T1ha2Hash64.ComputeHash),
        new Tuple64(nameof(Wy3Hash64), Wy3Hash64.ComputeIndex, Wy3Hash64.ComputeHash),
        new Tuple64(nameof(Wy4Hash64), Wy4Hash64.ComputeIndex, Wy4Hash64.ComputeHash),
        new Tuple64(nameof(XxHash64), XxHash64.ComputeIndex, XxHash64.ComputeHash),
        new Tuple64(nameof(Xx3Hash64), Xx3Hash64.ComputeIndex, Xx3Hash64.ComputeHash)
    ];

    public static TheoryData<Tuple128> Create128() =>
    [
        new Tuple128(nameof(AesniHash128), AesniHash128.ComputeIndex, AesniHash128.ComputeHash),
        new Tuple128(nameof(CityHash128), CityHash128.ComputeIndex, CityHash128.ComputeHash),
        new Tuple128(nameof(CityHashCrc128), CityHashCrc128.ComputeIndex, CityHashCrc128.ComputeHash),
        new Tuple128(nameof(FarmHash128), FarmHash128.ComputeIndex, FarmHash128.ComputeHash),
        new Tuple128(nameof(Gx2Hash128), Gx2Hash128.ComputeIndex, Gx2Hash128.ComputeHash),
        new Tuple128(nameof(MeowHash128), MeowHash128.ComputeIndex, MeowHash128.ComputeHash),
        new Tuple128(nameof(Murmur3Hash128), Murmur3Hash128.ComputeIndex, Murmur3Hash128.ComputeHash),
        new Tuple128(nameof(Xx3Hash128), Xx3Hash128.ComputeIndex, Xx3Hash128.ComputeHash)
    ];

    public record struct Tuple32(string Name, Func<uint, uint> Index, Func<ReadOnlySpan<byte>, uint> Hash) : IXunitSerializable
    {
        public void Deserialize(IXunitSerializationInfo info) => Name = info.GetValue<string>(nameof(Name));
        public void Serialize(IXunitSerializationInfo info) => info.AddValue(nameof(Name), Name);
        public override string ToString() => Name;
    }

    public record struct Tuple64(string Name, Func<ulong, ulong> Index, Func<ReadOnlySpan<byte>, ulong> Hash) : IXunitSerializable
    {
        public void Deserialize(IXunitSerializationInfo info) => Name = info.GetValue<string>(nameof(Name));
        public void Serialize(IXunitSerializationInfo info) => info.AddValue(nameof(Name), Name);
        public override string ToString() => Name;
    }

    public record struct Tuple128(string Name, Func<ulong, UInt128> Index, Func<ReadOnlySpan<byte>, UInt128> Hash) : IXunitSerializable
    {
        public void Deserialize(IXunitSerializationInfo info) => Name = info.GetValue<string>(nameof(Name));
        public void Serialize(IXunitSerializationInfo info) => info.AddValue(nameof(Name), Name);
        public override string ToString() => Name;
    }
}