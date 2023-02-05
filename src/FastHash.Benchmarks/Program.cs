using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Genbox.FastHash.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        IConfig config = DefaultConfig.Instance.AddJob(new Job(InfrastructureMode.InProcess));
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}