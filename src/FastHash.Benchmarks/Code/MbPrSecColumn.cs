using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Genbox.FastHash.Benchmarks.Code;

public class MbPrSecColumn : IColumn
{
    private readonly int _size;

    public MbPrSecColumn(int size)
    {
        _size = size;
    }

    public string Id => nameof(MbPrSecColumn);
    public string ColumnName => "MiB/s";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        Statistics? stats = summary[benchmarkCase].ResultStatistics;

        if (stats == null)
            return "?";

        //Number of operations as determined by the Count parameter

        int size = _size;

        if (size == 0)
            size = int.Parse(benchmarkCase.Parameters["Size"].ToString()!, NumberFormatInfo.InvariantInfo);

        //Mean is in nanoseconds, which is 1.000.000.000x less than a second
        double time = stats.Mean / 1000 / 1000 / 1000;

        double opsPrSec = (size / 1024f / 1024) / time;
        return opsPrSec.ToString("N0", NumberFormatInfo.InvariantInfo);
    }

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public int PriorityInCategory => 1;
    public override string ToString() => ColumnName;
    public string Legend => "Mibibytes pr. second";
}