namespace Genbox.FastHashesNet.FNVHash;

internal static class FNVConstants
{
    internal const uint FnvPrime = 0x01000193; //FNV_32_PRIME
    internal const uint FnvInit = 0x811c9dc5; //FNV1_32_INIT

    internal const ulong FnvPrime64 = 1099511628211; //FNV_32_PRIME
    internal const ulong FnvInit64 = 14695981039346656037; //FNV1_32_INIT
}