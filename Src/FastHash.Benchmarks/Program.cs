using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace Genbox.FastHash.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        IConfig config = ManualConfig.CreateMinimumViable()
                                     .AddJob(new Job(new RunMode
                                     {
                                         LaunchCount = 1,
                                         WarmupCount = 3,
                                         MinIterationCount = 3,
                                         MaxIterationCount = 10
                                     }, Job.InProcess))
                                     .AddAnalyser(EnvironmentAnalyser.Default,
                                         MinIterationTimeAnalyser.Default,
                                         RuntimeErrorAnalyser.Default,
                                         BaselineCustomAnalyzer.Default,
                                         HideColumnsAnalyser.Default)
                                     .AddValidator(BaselineValidator.FailOnError,
                                         SetupCleanupValidator.FailOnError,
                                         JitOptimizationsValidator.FailOnError,
                                         RunModeValidator.FailOnError,
                                         GenericBenchmarksValidator.DontFailOnError,
                                         DeferredExecutionValidator.FailOnError,
                                         ParamsAllValuesValidator.FailOnError,
                                         ParamsValidator.FailOnError)
                                     .WithOption(ConfigOptions.DisableLogFile, true);

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}