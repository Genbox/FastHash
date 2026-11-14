namespace Genbox.FastHash.TestShared;

public readonly record struct Index64Algorithm(string Name, Func<ulong, ulong> Index, Func<ReadOnlySpan<byte>, ulong> Hash)
{
    public bool IsSupported => AlgorithmCatalog.IsSupported(Name);

    public override string ToString() => Name;
}