namespace Genbox.FastHash.PolymurHash;

internal static class PolymurConstants
{
    internal const ulong POLYMUR_P611 = (1UL << 61) - 1;
    internal const ulong POLYMUR_ARBITRARY1 = 0x6a09e667f3bcc908UL; // Completely arbitrary, these
    internal const ulong POLYMUR_ARBITRARY2 = 0xbb67ae8584caa73bUL; // are taken from SHA-2, and
    internal const ulong POLYMUR_ARBITRARY3 = 0x3c6ef372fe94f82bUL; // are the fractional bits of
    internal const ulong POLYMUR_ARBITRARY4 = 0xa54ff53a5f1d36f1UL; // sqrt(p), p = 2, 3, 5, 7.
}