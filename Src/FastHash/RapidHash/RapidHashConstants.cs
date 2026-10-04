namespace Genbox.FastHash.RapidHash;

internal static class RapidHashConstants
{
    internal const ulong DefaultSecret0 = 0x2d358dccaa6c78a5UL;
    internal const ulong DefaultSecret1 = 0x8bb84b93962eacc9UL;
    internal const ulong DefaultSecret2 = 0x4b33a62ed433d4a3UL;
    internal const ulong DefaultSecret3 = 0x4d5a2da51de1aa47UL;
    internal const ulong DefaultSecret4 = 0xa0761d6478bd642fUL;
    internal const ulong DefaultSecret5 = 0xe7037ed1a0b428dbUL;
    internal const ulong DefaultSecret6 = 0x90ed1765281c388cUL;
    internal const ulong DefaultSecret7 = 0xaaaaaaaaaaaaaaaaUL;
    internal static readonly ulong[] DefaultSecret =
    [
        DefaultSecret0,
        DefaultSecret1,
        DefaultSecret2,
        DefaultSecret3,
        DefaultSecret4,
        DefaultSecret5,
        DefaultSecret6,
        DefaultSecret7
    ];
}