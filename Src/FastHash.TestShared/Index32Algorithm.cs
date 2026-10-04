namespace Genbox.FastHash.TestShared;

public readonly record struct Index32Algorithm(string Name, Func<uint, uint> Index, Func<ReadOnlySpan<byte>, uint> Hash)
{
    public override string ToString() => Name;
}