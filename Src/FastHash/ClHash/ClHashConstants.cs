#if NET8_0_OR_GREATER
// C# port of simdhash/clhash 1.0.0 (fd0331c) by Daniel Lemire and Owen Kaser.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.ClHash;

internal static class ClHashConstants
{
    internal const int Random64BitWordsNeeded = 133;
    internal const int RandomBytesNeeded = Random64BitWordsNeeded * sizeof(ulong);
    internal const int WordsPerBlock = 128;

    internal const ulong DefaultSeed1 = 137;
    internal const ulong DefaultSeed2 = 777;
}
#endif