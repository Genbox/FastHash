namespace Genbox.FastHash.TestShared;

public readonly record struct Index64Algorithm(string Name, Func<ulong, ulong> Index, Func<ReadOnlySpan<byte>, ulong> Hash)
{
    public override string ToString() => Name;
}