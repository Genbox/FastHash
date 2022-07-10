using BenchmarkDotNet.Running;

namespace FastHashesNet.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<HashBenchmark>();
    }
}