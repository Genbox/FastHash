using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Genbox.FastHash.Benchmarks.Code;

public class MbPrSecColumn : IColumn
{
    private readonly int _size;

    public MbPrSecColumn(int size = 0)
    {
        _size = size;
    }

    public string Id => nameof(MbPrSecColumn);
    public string ColumnName => "MiB/s";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(benchmarkCase);

        BenchmarkReport? report = summary.Reports.FirstOrDefault(x => ReferenceEquals(x.BenchmarkCase, benchmarkCase));
        Statistics? stats = report?.ResultStatistics;

        if (stats == null)
            return "?";

        int size = _size == 0 ? (int)benchmarkCase.Parameters["Size"] : _size;
        double opsPrSec = (size * (1_000_000_000.0 / (1024 * 1024))) / stats.Mean;
        return opsPrSec.ToString("N0", NumberFormatInfo.InvariantInfo);
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;
    public int PriorityInCategory => 1;
    public string Legend => "Mebibytes per second";
    public override string ToString() => ColumnName;
}