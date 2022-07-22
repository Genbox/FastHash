using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.xxHash;

internal static class ImplSwitch
{
    internal static unsafe void XXH3_accumulate_512(ulong* acc, byte* input, byte* secret)
    {
        if (Avx2.IsSupported)
            xxHashShared.XXH3_accumulate_512_avx2(acc, input, secret);
        else if (Sse2.IsSupported)
            xxHashShared.XXH3_accumulate_512_sse2(acc, input, secret);
        else
            xxHashShared.XXH3_accumulate_512_scalar(acc, input, secret);
    }

    internal static unsafe void XXH3_scrambleAcc(ulong* acc, byte* secret)
    {
        if (Avx2.IsSupported)
            xxHashShared.XXH3_scrambleAcc_avx2(acc, secret);
        else if (Sse2.IsSupported)
            xxHashShared.XXH3_scrambleAcc_sse2(acc, secret);
        else
            xxHashShared.XXH3_scrambleAcc_scalar(acc, secret);
    }

    internal static unsafe void XXH3_initCustomSecret(byte* customSecret, ulong seed)
    {
        if (Avx2.IsSupported)
            xxHashShared.XXH3_initCustomSecret_avx2(customSecret, seed);
        else if (Sse2.IsSupported)
            xxHashShared.XXH3_initCustomSecret_sse2(customSecret, seed);
        else
            xxHashShared.XXH3_initCustomSecret_scalar(customSecret, seed);
    }
}