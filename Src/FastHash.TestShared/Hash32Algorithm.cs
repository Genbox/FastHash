namespace Genbox.FastHash.TestShared;

public readonly record struct Hash32Algorithm(string Name, Func<ReadOnlySpan<byte>, uint>? Hash, Hash32Unsafe? UnsafeHash, byte[]? Expected)
{
    public bool IsSupported => AlgorithmCatalog.IsSupported(Name);

    public override string ToString() => Name;
}