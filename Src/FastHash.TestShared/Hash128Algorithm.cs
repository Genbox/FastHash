namespace Genbox.FastHash.TestShared;

public readonly record struct Hash128Algorithm(string Name, Func<ReadOnlySpan<byte>, UInt128>? Hash, Hash128Unsafe? UnsafeHash, UInt128? Expected)
{
    public bool IsSupported => AlgorithmCatalog.IsSupported(Name);

    public override string ToString() => Name;
}