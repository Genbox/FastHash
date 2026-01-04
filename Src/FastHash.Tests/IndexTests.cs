using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics.X86;
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

        for (int i = 0; i <= 32; i++)
        {
            uint value = 1u << i;
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

        for (int i = 0; i <= 64; i++)
        {
            ulong value = 1ul << i;
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

        for (int i = 0; i <= 64; i++)
        {
            ulong value = 1ul << i;
            UInt128 h1 = c.Index(value);

            BinaryPrimitives.WriteUInt64LittleEndian(valueBytes, value);
            UInt128 h2 = c.Hash(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    public static TheoryData<Tuple32> Create32()
    {
        TheoryData<Tuple32> data = new();

        data.Add(new Tuple32(nameof(CityHash32), CityHash32.ComputeIndex, CityHash32.ComputeHash));
        data.Add(new Tuple32(nameof(Djb2Hash32), Djb2Hash32.ComputeIndex, Djb2Hash32.ComputeHash));
        data.Add(new Tuple32(nameof(FarmHash32), FarmHash32.ComputeIndex, FarmHash32.ComputeHash));
        data.Add(new Tuple32(nameof(Fnv1aHash32), Fnv1aHash32.ComputeIndex, Fnv1aHash32.ComputeHash));
        data.Add(new Tuple32(nameof(GxHash32), static x => GxHash32.ComputeIndex(x), static x => GxHash32.ComputeHash(x)));
        data.Add(new Tuple32(nameof(Gx2Hash32), static x => Gx2Hash32.ComputeIndex(x, new UInt128(0, 0)), static x => Gx2Hash32.ComputeHash(x, new UInt128(0, 0))));
        data.Add(new Tuple32(nameof(MarvinHash32), MarvinHash32.ComputeIndex, static x => MarvinHash32.ComputeHash(x)));
        data.Add(new Tuple32(nameof(Murmur3Hash32), Murmur3Hash32.ComputeIndex, static x => Murmur3Hash32.ComputeHash(x)));
        data.Add(new Tuple32(nameof(SuperFastHash32), SuperFastHash32.ComputeIndex, SuperFastHash32.ComputeHash));
        data.Add(new Tuple32(nameof(Xx2Hash32), Xx2Hash32.ComputeIndex, static x => Xx2Hash32.ComputeHash(x)));

        return data;
    }

    public static TheoryData<Tuple64> Create64()
    {
        TheoryData<Tuple64> data = new();

        if (Aes.IsSupported)
            data.Add(new Tuple64(nameof(AesniHash64), static x => AesniHash64.ComputeIndex(x), static x => AesniHash64.ComputeHash(x)));

        data.Add(new Tuple64(nameof(CityHash64), CityHash64.ComputeIndex, CityHash64.ComputeHash));
        data.Add(new Tuple64(nameof(Djb2Hash64), Djb2Hash64.ComputeIndex, Djb2Hash64.ComputeHash));
        data.Add(new Tuple64(nameof(FarshHash64), static x => FarshHash64.ComputeIndex(x), static x => FarshHash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(FoldHash64), static x => FoldHash64.ComputeIndex(x, 0), static x => FoldHash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(FoldHashQuality64), static x => FoldHashQuality64.ComputeIndex(x, 0), static x => FoldHashQuality64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(FarmHash64), FarmHash64.ComputeIndex, static x => FarmHash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(Fnv1aHash64), Fnv1aHash64.ComputeIndex, Fnv1aHash64.ComputeHash));
        data.Add(new Tuple64(nameof(GxHash64), static x => GxHash64.ComputeIndex(x), static x => GxHash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(Gx2Hash64), static x => Gx2Hash64.ComputeIndex(x, new UInt128(0, 0)), static x => Gx2Hash64.ComputeHash(x, new UInt128(0, 0))));
        data.Add(new Tuple64(nameof(HighwayHash64Unsafe), HighwayHash64Unsafe.ComputeIndex, static x =>
        {
            unsafe
            {
                fixed (byte* ptr = x)
                    return HighwayHash64Unsafe.ComputeHash(ptr, x.Length);
            }
        }));
        data.Add(new Tuple64(nameof(MarvinHash64), static x => MarvinHash64.ComputeIndex(x), static x => MarvinHash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(MeowHash64Unsafe), MeowHash64Unsafe.ComputeIndex, static x =>
        {
            unsafe
            {
                fixed (byte* ptr = x)
                    return MeowHash64Unsafe.ComputeHash(ptr, x.Length);
            }
        }));
        data.Add(new Tuple64(nameof(Polymur2Hash64), Polymur2Hash64.ComputeIndex, static x => Polymur2Hash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(RapidHash64), static x => RapidHash64.ComputeIndex(x, 0), static x => RapidHash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(RapidHashMicro64), static x => RapidHashMicro64.ComputeIndex(x, 0), static x => RapidHashMicro64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(RapidHashNano64), static x => RapidHashNano64.ComputeIndex(x, 0), static x => RapidHashNano64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(SipHash64), static x => SipHash64.ComputeIndex(x), static x => SipHash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(Wy3Hash64), Wy3Hash64.ComputeIndex, static x => Wy3Hash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(Xx2Hash64), Xx2Hash64.ComputeIndex, static x => Xx2Hash64.ComputeHash(x)));
        data.Add(new Tuple64(nameof(Xx3Hash64), static x => Xx3Hash64.ComputeIndex(x), static x => Xx3Hash64.ComputeHash(x)));

        return data;
    }

    public static TheoryData<Tuple128> Create128()
    {
        TheoryData<Tuple128> data = new();

        if (Aes.IsSupported)
            data.Add(new Tuple128(nameof(AesniHash128), static x => AesniHash128.ComputeIndex(x), static x => AesniHash128.ComputeHash(x)));

        data.Add(new Tuple128(nameof(CityHash128), static x => CityHash128.ComputeIndex(x), CityHash128.ComputeHash));
        data.Add(new Tuple128(nameof(GxHash128), static x => GxHash128.ComputeIndex(x), static x => GxHash128.ComputeHash(x)));
        data.Add(new Tuple128(nameof(Gx2Hash128), static x => Gx2Hash128.ComputeIndex(x, new UInt128(0, 0)), static x => Gx2Hash128.ComputeHash(x, new UInt128(0, 0))));
        data.Add(new Tuple128(nameof(MeowHash128Unsafe), MeowHash128Unsafe.ComputeIndex, static x =>
        {
            unsafe
            {
                fixed (byte* ptr = x)
                    return MeowHash128Unsafe.ComputeHash(ptr, x.Length);
            }
        }));
        data.Add(new Tuple128(nameof(Murmur3Hash128), static x => Murmur3Hash128.ComputeIndex(x), static x => Murmur3Hash128.ComputeHash(x)));
        data.Add(new Tuple128(nameof(Xx3Hash128), static x => Xx3Hash128.ComputeIndex(x), static x => Xx3Hash128.ComputeHash(x)));

        return data;
    }

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