namespace Genbox.FastHash.TestShared;

public readonly record struct Hash256Algorithm(string Name, Hash256? Hash, Hash256Unsafe? UnsafeHash)
{
    public override string ToString() => Name;
}