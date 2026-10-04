// C# port of wangyi-fudan/wyhash 3.0.0 (9f68c1b) and 4.3.0 (2ac9a50) by Wang Yi.
// Distributed under the Unlicense; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.WyHash;

internal static class WyHashConstants
{
    internal static readonly ulong[] V3DefaultSecret = [0xa0761d6478bd642ful, 0xe7037ed1a0b428dbul, 0x8ebc6af09c88c6e3ul, 0x589965cc75374cc3ul];
    internal static readonly ulong[] V4DefaultSecret = [0x2d358dccaa6c78a5UL, 0x8bb84b93962eacc9UL, 0x4b33a62ed433d4a3UL, 0x4d5a2da51de1aa47UL];
}