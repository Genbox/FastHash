// C# port of google/highwayhash (f8381f3). Copyright 2017 Google Inc. All Rights Reserved.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.HighwayHash;

internal static class HighwayHashConstants
{
    internal const ulong DefaultKey0 = 0x0706050403020100UL;
    internal const ulong DefaultKey1 = 0x0F0E0D0C0B0A0908UL;
    internal const ulong DefaultKey2 = 0x1716151413121110UL;
    internal const ulong DefaultKey3 = 0x1F1E1D1C1B1A1918UL;
}