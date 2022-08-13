using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Genbox.FastHash.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        Job shortInProcess = new Job(InfrastructureMode.InProcess);

        ManualConfig config = ManualConfig.Create(DefaultConfig.Instance.AddJob(shortInProcess));

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                         .Run(args, config);
    }
}