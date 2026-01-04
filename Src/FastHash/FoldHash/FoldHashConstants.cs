namespace Genbox.FastHash.FoldHash;

internal static class FoldHashConstants
{
    // Arbitrary constants with high entropy (hex digits of pi).
    internal const ulong ARBITRARY0 = 0x243f6a8885a308d3UL;
    internal const ulong ARBITRARY1 = 0x13198a2e03707344UL;
    internal const ulong ARBITRARY2 = 0xa4093822299f31d0UL;
    internal const ulong ARBITRARY3 = 0x082efa98ec4e6c89UL;
    internal const ulong ARBITRARY4 = 0x452821e638d01377UL;
    internal const ulong ARBITRARY5 = 0xbe5466cf34e90c6cUL;
    internal const ulong ARBITRARY6 = 0xc0ac29b7c97c50ddUL;
    internal const ulong ARBITRARY7 = 0x3f84d5b5b5470917UL;
    internal const ulong ARBITRARY8 = 0x9216d5d98979fb1bUL;
    internal const ulong ARBITRARY9 = 0xd1310ba698dfb5acUL;
    internal const ulong ARBITRARY10 = 0x2ffd72dbd01adfb7UL;
    internal const ulong ARBITRARY11 = 0xb8e1afed6a267e96UL;

    internal static readonly ulong[] DefaultSharedSeed =
    [
        ARBITRARY6,
        ARBITRARY7,
        ARBITRARY8,
        ARBITRARY9,
        ARBITRARY10,
        ARBITRARY11
    ];
}
