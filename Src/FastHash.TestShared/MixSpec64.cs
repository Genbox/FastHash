using System.Runtime.CompilerServices;

namespace Genbox.FastHash.TestShared;

public record MixSpec64(Func<ulong, ulong> Func, [CallerArgumentExpression(nameof(Func))]string Name = "")
{
    public override string ToString() => Name;
}