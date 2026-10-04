// C# port of wangyi-fudan/wyhash final version 2 (59aacba), final version 3 (991aa3d) and 4.3.0 (2ac9a50) by Wang Yi.
// Distributed under the Unlicense; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.WyHash;

internal static class WyHashConstants
{
    internal static readonly ulong[] V2DefaultSecret = [0xa0761d6478bd642ful, 0xe7037ed1a0b428dbul, 0x8ebc6af09c88c6e3ul, 0x589965cc75374cc3ul];

    // Final versions 2 and 3 share the same default secret.
    internal static readonly ulong[] V3DefaultSecret = V2DefaultSecret;

    internal static readonly ulong[] V4DefaultSecret = [0x2d358dccaa6c78a5UL, 0x8bb84b93962eacc9UL, 0x4b33a62ed433d4a3UL, 0x4d5a2da51de1aa47UL];
}