namespace Genbox.FastHash.TestShared;

public readonly record struct Hash64Algorithm(string Name, Func<ReadOnlySpan<byte>, ulong>? Hash, Hash64Unsafe? UnsafeHash, byte[]? Expected)
{
    public override string ToString() => Name;
}