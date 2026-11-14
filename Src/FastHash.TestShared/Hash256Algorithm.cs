namespace Genbox.FastHash.TestShared;

public readonly record struct Hash256Algorithm(string Name, Hash256? Hash, Hash256Unsafe? UnsafeHash)
{
    public bool IsSupported => AlgorithmCatalog.IsSupported(Name);

    public override string ToString() => Name;
}