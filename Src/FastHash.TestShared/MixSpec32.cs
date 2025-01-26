using System.Runtime.CompilerServices;

namespace Genbox.FastHash.TestShared;

public record MixSpec32(Func<uint, uint> Func, [CallerArgumentExpression(nameof(Func))]string Name = "")
{
    public override string ToString() => Name;
}