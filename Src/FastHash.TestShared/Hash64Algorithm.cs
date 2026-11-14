namespace Genbox.FastHash.TestShared;

public readonly record struct Hash64Algorithm(string Name, Func<ReadOnlySpan<byte>, ulong>? Hash, Hash64Unsafe? UnsafeHash, ulong? Expected)
{
    public bool IsSupported => AlgorithmCatalog.IsSupported(Name);

    public override string ToString() => Name;
}