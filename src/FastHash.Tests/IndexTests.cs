using System.Buffers.Binary;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Tests;

public class IndexTests
{
    public static IEnumerable<object[]> CreateAlgorithms32()
    {
        yield return new object[] { nameof(CityHash32), (uint val) => CityHash32.ComputeIndex(val), (byte[] val) => CityHash32.ComputeHash(val) };
        yield return new object[] { nameof(Djb2Hash32), (uint val) => Djb2Hash32.ComputeIndex(val), (byte[] val) => Djb2Hash32.ComputeHash(val) };
        yield return new object[] { nameof(FarmHash32), (uint val) => FarmHash32.ComputeIndex(val), (byte[] val) => FarmHash32.ComputeHash(val) };
        yield return new object[] { nameof(Fnv1aHash32), (uint val) => Fnv1aHash32.ComputeIndex(val), (byte[] val) => Fnv1aHash32.ComputeHash(val) };
        yield return new object[] { nameof(MarvinHash32), (uint val) => MarvinHash32.ComputeIndex(val), (byte[] val) => MarvinHash32.ComputeHash(val) };
        yield return new object[] { nameof(Murmur3Hash32), (uint val) => Murmur3Hash32.ComputeIndex(val), (byte[] val) => Murmur3Hash32.ComputeHash(val) };
        yield return new object[] { nameof(SuperFastHash32), (uint val) => SuperFastHash32.ComputeIndex(val), (byte[] val) => SuperFastHash32.ComputeHash(val) };
        yield return new object[] { nameof(Xx2Hash32), (uint val) => Xx2Hash32.ComputeIndex(val), (byte[] val) => Xx2Hash32.ComputeHash(val) };
    }

    public static IEnumerable<object[]> CreateAlgorithms64()
    {
        yield return new object[] { nameof(CityHash64), (ulong val) => CityHash64.ComputeIndex(val), (byte[] val) => CityHash64.ComputeHash(val) };
        yield return new object[] { nameof(Djb2Hash64), (ulong val) => Djb2Hash64.ComputeIndex(val), (byte[] val) => Djb2Hash64.ComputeHash(val) };
        yield return new object[] { nameof(Fnv1aHash64), (ulong val) => Fnv1aHash64.ComputeIndex(val), (byte[] val) => Fnv1aHash64.ComputeHash(val) };
        yield return new object[] { nameof(Wy3Hash64), (ulong val) => Wy3Hash64.ComputeIndex(val), (byte[] val) => Wy3Hash64.ComputeHash(val) };
        yield return new object[] { nameof(Xx2Hash64), (ulong val) => Xx2Hash64.ComputeIndex(val), (byte[] val) => Xx2Hash64.ComputeHash(val) };
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms32))]
    public void Check32(string _, Func<uint, uint> func1, Func<byte[], uint> func2)
    {
        byte[] valueBytes = new byte[4];

        for (int i = 0; i <= 32; i++)
        {
            uint value = 1u << i;
            uint h1 = func1(value);

            BinaryPrimitives.WriteUInt32LittleEndian(valueBytes, value);
            uint h2 = func2(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms64))]
    public void Check64(string _, Func<ulong, ulong> func1, Func<byte[], ulong> func2)
    {
        byte[] valueBytes = new byte[8];

        for (int i = 0; i <= 64; i++)
        {
            ulong value = 1ul << i;
            ulong h1 = func1(value);

            BinaryPrimitives.WriteUInt64LittleEndian(valueBytes, value);
            ulong h2 = func2(valueBytes);

            Assert.Equal(h2, h1);
        }
    }
}