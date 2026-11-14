using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using Genbox.FastHash.TestShared;

namespace Genbox.FastHash.Benchmarks.Code;

internal sealed class HardwareFilter : IFilter
{
    public bool Predicate(BenchmarkCase benchmarkCase)
    {
        string method = benchmarkCase.Descriptor.WorkloadMethod.Name;
        if (benchmarkCase.Descriptor.Type == typeof(Multiply64Benchmarks) && method == nameof(Multiply64Benchmarks.Bmi2Mul))
            return Bmi2.X64.IsSupported;

        if (benchmarkCase.Descriptor.Type == typeof(SeededHashBenchmarks))
        {
            if (method.StartsWith("ClHash", StringComparison.Ordinal))
                return AlgorithmCatalog.IsSupported("ClHash64");
            if (method.StartsWith("PolyXor", StringComparison.Ordinal))
                return AlgorithmCatalog.IsSupported("PolyXorHash128");
        }

        string algorithm = method.EndsWith("Test", StringComparison.Ordinal) ? method[..^4] : method;
        return AlgorithmCatalog.IsSupported(algorithm);
    }
}