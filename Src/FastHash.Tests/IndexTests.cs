using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MurmurHash;
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

    public static TheoryData<Tuple32> Create32() =>
    [
        new Tuple32(nameof(CityHash32), CityHash32.ComputeIndex, CityHash32.ComputeHash),
        new Tuple32(nameof(Djb2Hash32), Djb2Hash32.ComputeIndex, Djb2Hash32.ComputeHash),
        new Tuple32(nameof(FarmHash32), FarmHash32.ComputeIndex, FarmHash32.ComputeHash),
        new Tuple32(nameof(Fnv1aHash32), Fnv1aHash32.ComputeIndex, Fnv1aHash32.ComputeHash),
        new Tuple32(nameof(MarvinHash32), MarvinHash32.ComputeIndex, static x => MarvinHash32.ComputeHash(x)),
        new Tuple32(nameof(Murmur3Hash32), Murmur3Hash32.ComputeIndex, static x => Murmur3Hash32.ComputeHash(x)),
        new Tuple32(nameof(SuperFastHash32), SuperFastHash32.ComputeIndex, SuperFastHash32.ComputeHash),
        new Tuple32(nameof(Xx2Hash32), Xx2Hash32.ComputeIndex, static x => Xx2Hash32.ComputeHash(x))
    ];

    public static TheoryData<Tuple64> Create64() =>
    [
        new Tuple64(nameof(CityHash64), CityHash64.ComputeIndex, CityHash64.ComputeHash),
        new Tuple64(nameof(Djb2Hash64), Djb2Hash64.ComputeIndex, Djb2Hash64.ComputeHash),
        new Tuple64(nameof(FarmHash64), FarmHash64.ComputeIndex, static x => FarmHash64.ComputeHash(x)),
        new Tuple64(nameof(Fnv1aHash64), Fnv1aHash64.ComputeIndex, Fnv1aHash64.ComputeHash),
        new Tuple64(nameof(SipHash64), static x => SipHash64.ComputeIndex(x), static x => SipHash64.ComputeHash(x)),
        new Tuple64(nameof(Wy3Hash64), Wy3Hash64.ComputeIndex, static x => Wy3Hash64.ComputeHash(x)),
        new Tuple64(nameof(Xx2Hash64), Xx2Hash64.ComputeIndex, static x => Xx2Hash64.ComputeHash(x))
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
}