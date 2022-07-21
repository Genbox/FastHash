using BenchmarkDotNet.Attributes;

namespace Genbox.FastHash.Benchmarks.Code;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class MbPrSecColumnAttribute : ColumnConfigBaseAttribute
{
    public MbPrSecColumnAttribute() : base(new MbPrSecColumn()) {}
}