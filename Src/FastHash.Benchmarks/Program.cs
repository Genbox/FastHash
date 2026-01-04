using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
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
                                         MinIterationCount = 10,
                                         MaxIterationCount = 20
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
                                     .WithOption(ConfigOptions.DisableLogFile, true)
                                     .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(50));

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}