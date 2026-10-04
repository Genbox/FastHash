// C# implementation of FNV-1a by Glenn Fowler, Landon Curt Noll and Kiem-Phong Vo, which is in the public domain.
// See THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.FnvHash;

internal static class FnvHashConstants
{
    internal const uint FNV_32_PRIME = 0x1000193;
    internal const uint FNV1_32_INIT = 0x811C9DC5;

    internal const ulong FNV_64_PRIME = 0x100000001B3;
    internal const ulong FNV1_64_INIT = 0xCBF29CE484222325;
}