namespace Genbox.FastHash.TestShared;

public readonly record struct Index128Algorithm(string Name, Func<ulong, UInt128> Index, Func<ReadOnlySpan<byte>, UInt128> Hash)
{
    public override string ToString() => Name;
}