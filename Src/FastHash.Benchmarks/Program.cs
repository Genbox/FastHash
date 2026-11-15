using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Genbox.FastHash.Benchmarks.Code;

namespace Genbox.FastHash.Benchmarks;

internal static class Program
{
    public static void Main(string[] args)
    {
        IConfig config = ManualConfig.CreateMinimumViable()
                                     .AddFilter(new HardwareFilter())
                                     .AddJob(new Job(new RunMode
                                     {
                                         LaunchCount = 1,
                                         WarmupCount = 3,
                                         MinIterationCount = 10,
                                         MaxIterationCount = 20
                                     }, Job.Default)
                                         // Keep the driver independent of cold-call/JIT timing.
                                         .WithUnrollFactor(1)
                                         .WithEvaluateOverhead(true))
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
                                     .WithOption(ConfigOptions.DisableLogFile, true)
                                     .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(50));

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}