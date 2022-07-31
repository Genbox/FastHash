using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[InProcess]
public class ReadUnalignedBenchmarks
{
    private byte[] _testData;

    [Params(7, 31, 127, 1023, 1024 * 1024 * 31)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _testData = BenchmarkHelper.GetRandomBytes(Size);
    }

    [Benchmark(Baseline = true)]
    public ulong OneAtTheTime()
    {
        ulong acc = 0;

        for (int i = 0; i < _testData.Length; i++)
            acc += _testData[i];

        return acc;
    }

    [Benchmark]
    public ulong OneAtTheTimeUnsafe()
    {
        ulong acc = 0;

        ref byte ptr = ref _testData[0];

        for (int i = 0; i < _testData.Length; i++)
            acc += Unsafe.Add(ref ptr, i); //This avoids the bounds check

        return acc;
    }

    [Benchmark]
    public ulong UnalignedIntegers32()
    {
        ulong acc = 0;

        uint[] intArr = Unsafe.As<byte[], uint[]>(ref _testData);

        int count = _testData.Length;
        uint chunks = (uint)Math.DivRem(count, sizeof(uint), out int rem);

        while (chunks > 0)
        {
            acc += intArr[chunks];
            chunks--;
        }

        byte[] localPtr = _testData;

        //There is 0-3 byes left. Read 2.
        if ((rem & 0b_0010) != 0)
        {
            acc += localPtr[rem];
            acc += localPtr[rem - 1];
            rem -= 2;
        }

        //Read last byte if any
        if ((rem & 0b_0001) != 0)
            acc += localPtr[rem];

        return acc;
    }

    [Benchmark]
    public ulong UnalignedIntegers64()
    {
        ulong acc = 0;

        ulong[] intArr = Unsafe.As<byte[], ulong[]>(ref _testData);

        int count = _testData.Length;
        uint chunks = (uint)Math.DivRem(count, sizeof(ulong), out int rem); //Cast to uint to have the while below do an optimized check

        while (chunks > 0u)
        {
            acc += intArr[chunks];
            chunks--;
        }

        byte[] localPtr = _testData; //Local reference avoids bounds check

        //There is 0-7 byes left. Read 4.
        if ((rem & 0b_0100) != 0)
        {
            acc += localPtr[rem];
            acc += localPtr[rem - 1];
            acc += localPtr[rem - 2];
            acc += localPtr[rem - 3];
            rem -= 4;
        }

        //There is 0-3 byes left. Read 2.
        if ((rem & 0b_0010) != 0)
        {
            acc += localPtr[rem];
            acc += localPtr[rem - 1];
            rem -= 2;
        }

        //Read last byte if any
        if ((rem & 0b_0001) != 0)
            acc += localPtr[rem];

        return acc;
    }
}