namespace Genbox.FastHash.HighwayHash;

public static class HighwayHashConstants
{
    internal const ulong DefaultKey0 = 0x0706050403020100UL;
    internal const ulong DefaultKey1 = 0x0F0E0D0C0B0A0908UL;
    internal const ulong DefaultKey2 = 0x1716151413121110UL;
    internal const ulong DefaultKey3 = 0x1F1E1D1C1B1A1918UL;

    public static ulong[] DefaultKeys => [DefaultKey0, DefaultKey1, DefaultKey2, DefaultKey3];
}