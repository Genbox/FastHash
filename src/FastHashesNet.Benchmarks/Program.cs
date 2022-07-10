﻿using BenchmarkDotNet.Running;

namespace Genbox.FastHashesNet.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<HashBenchmark>();
    }
}