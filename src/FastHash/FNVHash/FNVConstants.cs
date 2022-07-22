namespace Genbox.FastHash.FNVHash;

internal static class FNVConstants
{
    internal const uint FnvPrime = 0x1000193; //FNV_32_PRIME
    internal const uint FnvInit = 0x811C9DC5; //FNV1_32_INIT

    internal const ulong FnvPrime64 = 0x100000001B3; //FNV_64_PRIME
    internal const ulong FnvInit64 = 0xCBF29CE484222325; //FNV1_64_INIT
}