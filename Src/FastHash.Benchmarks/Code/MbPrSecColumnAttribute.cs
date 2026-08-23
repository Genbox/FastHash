namespace Genbox.FastHash.Benchmarks.Code;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class MbPrSecColumnAttribute : ColumnConfigBaseAttribute
{
    public MbPrSecColumnAttribute(int size = 0) : base(new MbPrSecColumn(size))
    {
        Size = size;
    }

    public int Size { get; }
}