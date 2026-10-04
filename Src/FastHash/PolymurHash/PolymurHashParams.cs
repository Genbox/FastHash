// C# port of orlp/polymur-hash 2.0.0 (c6cc688). Copyright (c) 2023 Orson Peters.
// Distributed under the zlib license; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.PolymurHash;

/// <summary>Contains immutable, seed-derived parameters for <see cref="Polymur2Hash64" />.</summary>
public sealed class PolymurHashParams
{
    internal readonly Polymur2Hash64.Parameters Parameters;

    /// <summary>Initializes parameters derived from <paramref name="seed" />.</summary>
    /// <param name="seed">The seed from which to derive the parameters.</param>
    public PolymurHashParams(ulong seed) => Parameters = Polymur2Hash64.CreateParams(seed);
}