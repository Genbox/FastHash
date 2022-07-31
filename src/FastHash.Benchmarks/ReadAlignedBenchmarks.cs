using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[InProcess]
public class ReadAlignedBenchmarks
{
    private byte[] _testData;

    [Params(8, 32, 128, 1024, 1024 * 1024 * 32)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _testData = BenchmarkHelper.GetRandomBytes(Size);
    }

    [Benchmark]
    public ulong OneAtTheTime()
    {
        ulong acc = 0;

        for (int i = 0; i < _testData.Length; i++)
            acc += _testData[i];

        return acc;
    }

    [Benchmark]
    public ulong AlignedFourAtTheTime()
    {
        ulong acc = 0;

        for (int i = 0; i < _testData.Length; i += 4)
        {
            acc += _testData[i];
            acc += _testData[i + 1];
            acc += _testData[i + 2];
            acc += _testData[i + 3];
        }

        return acc;
    }

    [Benchmark]
    public ulong AlignedIntegers32()
    {
        ulong acc = 0;

        uint[] intArr = Unsafe.As<byte[], uint[]>(ref _testData);
        int items = _testData.Length / sizeof(uint);

        for (int i = 0; i < items; i++)
            acc += intArr[i];

        return acc;
    }

    [Benchmark]
    public ulong AlignedIntegers64()
    {
        ulong acc = 0;

        ulong[] intArr = Unsafe.As<byte[], ulong[]>(ref _testData);
        int items = _testData.Length / sizeof(ulong);

        for (int i = 0; i < items; i++)
            acc += intArr[i];

        return acc;
    }
}