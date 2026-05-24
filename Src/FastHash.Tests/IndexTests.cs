using System.Buffers.Binary;
using Genbox.FastHash.TestShared;

namespace Genbox.FastHash.Tests;

public class IndexTests
{
    [Theory]
    [MemberData(nameof(Create32))]
    public void Check32(string name)
    {
        Index32Algorithm algorithm = AlgorithmCatalog.GetIndex32(name);
        byte[] valueBytes = new byte[4];

        uint[] values = [0u, uint.MaxValue, 0x5555_5555u, 0xAAAA_AAAAu, 0x0123_4567u, 0xFEDC_BA98u];

        foreach (uint value in values)
            AssertIndex(value);

        for (int i = 0; i < 32; i++)
            AssertIndex(1u << i);

        void AssertIndex(uint value)
        {
            uint h1 = algorithm.Index(value);

            BinaryPrimitives.WriteUInt32LittleEndian(valueBytes, value);
            uint h2 = algorithm.Hash(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    [Theory]
    [MemberData(nameof(Create64))]
    public void Check64(string name)
    {
        Index64Algorithm algorithm = AlgorithmCatalog.GetIndex64(name);
        byte[] valueBytes = new byte[8];

        ulong[] values = [0UL, ulong.MaxValue, 0x5555_5555_5555_5555UL, 0xAAAA_AAAA_AAAA_AAAAUL, 0x0123_4567_89AB_CDEFUL, 0xFEDC_BA98_7654_3210UL];

        foreach (ulong value in values)
            AssertIndex(value);

        for (int i = 0; i < 64; i++)
            AssertIndex(1UL << i);

        void AssertIndex(ulong value)
        {
            ulong h1 = algorithm.Index(value);

            BinaryPrimitives.WriteUInt64LittleEndian(valueBytes, value);
            ulong h2 = algorithm.Hash(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    [Theory]
    [MemberData(nameof(Create128))]
    public void Check128(string name)
    {
        Index128Algorithm algorithm = AlgorithmCatalog.GetIndex128(name);
        byte[] valueBytes = new byte[8];

        ulong[] values = [0UL, ulong.MaxValue, 0x5555_5555_5555_5555UL, 0xAAAA_AAAA_AAAA_AAAAUL, 0x0123_4567_89AB_CDEFUL, 0xFEDC_BA98_7654_3210UL];

        foreach (ulong value in values)
            AssertIndex(value);

        for (int i = 0; i < 64; i++)
            AssertIndex(1UL << i);

        void AssertIndex(ulong value)
        {
            UInt128 h1 = algorithm.Index(value);

            BinaryPrimitives.WriteUInt64LittleEndian(valueBytes, value);
            UInt128 h2 = algorithm.Hash(valueBytes);

            Assert.Equal(h2, h1);
        }
    }

    public static TheoryData<string> Create32() => Create(AlgorithmCatalog.Index32Algorithms.Select(static x => x.Name));

    public static TheoryData<string> Create64() => Create(AlgorithmCatalog.Index64Algorithms.Select(static x => x.Name));

    public static TheoryData<string> Create128() => Create(AlgorithmCatalog.Index128Algorithms.Select(static x => x.Name));

    private static TheoryData<string> Create(IEnumerable<string> names)
    {
        TheoryData<string> data = [];

        foreach (string name in names)
            data.Add(name);

        return data;
    }
}