# FastHash - High-performance non-cryptographic hashes

[![NuGet](https://img.shields.io/nuget/v/Genbox.FastHash.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Genbox.FastHash/)

### Features

* API supports [ReadOnlySpan](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1) for optimal performance and flexibility
* Most of the hashes are verified with test vectors from the original author
* High-performance zero-allocation implementations
* Index variants of each hash function that give the same output as if using the hash function, but takes in a 32bit/64bit integer.

### Hashes

These hash functions are included in the library.

| Name                                                                                                                                                    |                           Version                           | Authors                                                         | License               |
|---------------------------------------------------------------------------------------------------------------------------------------------------------|:-----------------------------------------------------------:|-----------------------------------------------------------------|-----------------------|
| [AbslHash](https://github.com/abseil/abseil-cpp/)                                                                                                       |                              -                              | The Abseil Authors                                              | Apache 2.0            |
| [AesniHash](https://github.com/PeterRK/PageBloomFilter)                                                                                                 |                          `ae11846`                          | Ruan Kunliang                                                   | BSD 3-Clause          |
| [CityHash](https://github.com/google/cityhash)                                                                                                          |                      1.1.1 (`4726e30`)                      | Geoff Pike, Jyrki Alakuijala                                    | MIT                   |
| [CLHash](https://github.com/simdhash/clhash)                                                                                                            |                      1.0.0 (`fd0331c`)                      | Daniel Lemire, Owen Kaser                                       | Apache 2.0            |
| [DJBHash](http://www.cse.yorku.ca/~oz/hash.html)                                                                                                        |                              -                              | Daniel J. Bernstein                                             | Public domain         |
| [FarmHash](https://github.com/google/farmhash)                                                                                                          |                      1.1.0 (`0d859a8`)                      | Geoff Pike                                                      | MIT                   |
| [FarshHash](https://github.com/Bulat-Ziganshin/FARSH)                                                                                                   |                      0.2.0 (`d74ef3a`)                      | Bulat Ziganshin                                                 | MIT                   |
| [FNVHash](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)                                                                   |                              -                              | Glenn Fowler, Landon Curt Noll, Kiem-Phong Vo                   | Public domain         |
| [FoldHash](https://github.com/orlp/foldhash)                                                                                                            |                      0.2.0 (`8f878c6`)                      | Orson Peters                                                    | Zlib                  |
| [Gx2Hash](https://github.com/ogxd/gxhash)                                                                                                               |                      3.5.0 (`6438a7b`)                      | Orso G.                                                         | MIT                   |
| [HighwayHash](https://github.com/google/highwayhash)                                                                                                    |                              -                              | Jyrki Alakuijala, Bill Cox, Jan Wassenberg                      | Apache 2.0            |
| [MarvinHash](https://github.com/dotnet/runtime/blob/4017327955f1d8ddc43980eb1848c52fbb131dfc/src/libraries/System.Private.CoreLib/src/System/Marvin.cs) |                          `4017327`                          | Niels Ferguson, Reid Borsuk, Jeffrey Cooperstein, Matthew Ellis | MIT                   |
| [MeowHash](https://github.com/cmuratori/meow_hash)                                                                                                      |                   0.5/calico (`b080caa`)                    | Molly Rocket, Inc.                                              | Zlib                  |
| [MurmurHash](https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp)                                                                      |                      3.0.0 (`92cf370`)                      | Austin Appleby                                                  | Public domain         |
| [PolymurHash](https://github.com/orlp/polymur-hash)                                                                                                     |                      2.0.0 (`c6cc688`)                      | Orson Peters                                                    | Zlib                  |
| [PolyXorHash](https://github.com/orlp/polyxor)                                                                                                          |                      0.1.0 (`3123eb6`)                      | Orson Peters                                                    | Zlib                  |
| [RapidHash](https://github.com/Nicoshev/rapidhash)                                                                                                      |                      3.0.0 (`bc4b4ba`)                      | Nicolas De Carli                                                | MIT                   |
| [SipHash](https://github.com/veorq/SipHash)                                                                                                             |                      1.0.0 (`dfaa38e`)                      | Jean-Philippe Aumasson, Daniel J. Bernstein                     | MIT                   |
| [SuperFastHash](http://www.azillionmonkeys.com/qed/hash.html)                                                                                           |                              -                              | Paul Hsieh                                                      | Paul Hsieh derivative |
| [T1ha2Hash](https://github.com/erthink/t1ha)                                                                                                            |                      2.1.1 (`0db5d6b`)                      | Positive Technologies, Leonid Yuriev                            | Zlib                  |
| [WyHash](https://github.com/wangyi-fudan/wyhash)                                                                                                        | final 2 (`59aacba`), final 3 (`991aa3d`), 4.3.0 (`2ac9a50`) | Wang Yi                                                         | Unlicense             |
| [xxHash](https://github.com/Cyan4973/xxHash)                                                                                                            |                      0.8.3 (`e626a72`)                      | Yann Collet                                                     | BSD 2-Clause          |

### License

FastHash is licensed under the [MIT license](LICENSE.txt).

The hash functions are ports of the projects listed above and remain subject to their original licenses. The license column shows the license FastHash uses where upstream offers a choice. See [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) for the full notices.

### Implementation status

The table below gives an overview of the implementations. Every one is verified against upstream test vectors.

| Name          | 32bit | 64bit | 128bit | Index | Seeded | Secret | 
|---------------|:-----:|:-----:|:------:|:-----:|:------:|:------:|
| AbslHash      |       |   x   |        |       |   x    |        | 
| AesniHash     |       |   x   |   x    |   x   |   x    |        | 
| CityHash      |   x   |   x   |   x    |   x   |   x    |        | 
| CLHash        |       |   x   |        |       |   x    |   x    | 
| DJBHash       |   x   |   x   |        |   x   |        |        | 
| FarmHash      |   x   |   x   |   x    |   x   |   x    |        | 
| FarshHash     |   x   |   x   |        |   x   |   x    |        | 
| FNVHash       |   x   |   x   |        |   x   |        |        | 
| FoldHash      |       |   x   |        |   x   |   x    |   x    | 
| Gx2Hash       |   x   |   x   |   x    |   x   |   x    |        | 
| HighwayHash   |       |   x   |        |   x   |   x    |   x    | 
| MarvinHash    |   x   |   x   |        |   x   |   x    |        | 
| MeowHash      |       |   x   |   x    |   x   |        |        | 
| MurmurHash    |   x   |       |   x    |   x   |   x    |        | 
| PolymurHash   |       |   x   |        |   x   |   x    |        | 
| PolyXorHash   |       |       |   x    |   x   |   x    |   x    | 
| RapidHash     |       |   x   |        |   x   |   x    |        | 
| SipHash       |       |   x   |        |   x   |   x    |   x    | 
| SuperFastHash |   x   |       |        |   x   |   x    |        | 
| T1ha2Hash     |       |   x   |        |   x   |   x    |        | 
| WyHash2       |       |   x   |        |   x   |   x    |   x    | 
| WyHash3       |       |   x   |        |   x   |   x    |   x    | 
| WyHash4       |       |   x   |        |   x   |   x    |   x    | 
| xxHash        |   x   |   x   |        |   x   |   x    |        | 
| xxHash3       |       |   x   |   x    |   x   |   x    |        | 

* **Bits:** Output widths exposed by the implementation.
* **Index:** Hashes a 32-bit or 64-bit integer directly.
* **Seeded:** Accepts an input seed to vary hash results.
* **Secret:** Accepts caller-provided key material.

### Performance

#### Hash functions

Measured on a repeatedly hashed 32 KiB buffer filled using `Random(42)`. MD5, SHA1 and SHA256 are .NET `System.Security.Cryptography` comparison references.

| Method            |        Mean |       Error |      StdDev |   MiB/s |
|-------------------|------------:|------------:|------------:|--------:|
| Gx2Hash32         |    279.8 ns |     5.57 ns |     4.03 ns | 111,687 |
| Gx2Hash64         |    291.2 ns |    15.23 ns |    11.89 ns | 107,315 |
| Gx2Hash128        |    293.1 ns |    10.01 ns |     7.81 ns | 106,619 |
| MeowHash64        |    549.1 ns |     5.16 ns |     3.73 ns |  56,911 |
| MeowHash128       |    551.0 ns |     8.01 ns |     5.79 ns |  56,715 |
| Xx3Hash64         |    577.7 ns |    32.84 ns |    23.74 ns |  54,094 |
| PolyXorHash128    |    588.3 ns |    38.12 ns |    25.21 ns |  53,119 |
| Xx3Hash128        |    592.2 ns |    21.79 ns |    17.01 ns |  52,769 |
| AbslHash64        |    798.5 ns |     4.39 ns |     3.43 ns |  39,136 |
| Rapid3Hash64      |    844.8 ns |    16.72 ns |    13.05 ns |  36,991 |
| FoldHashQuality64 |    868.2 ns |    32.49 ns |    23.49 ns |  35,994 |
| AesniHash128      |    873.9 ns |     4.88 ns |     3.23 ns |  35,759 |
| AesniHash64       |    889.8 ns |     5.20 ns |     3.76 ns |  35,120 |
| FoldHash64        |    900.7 ns |    84.40 ns |    65.90 ns |  34,695 |
| Wy3Hash64         |    925.6 ns |    11.96 ns |     9.33 ns |  33,762 |
| Rapid3HashNano64  |    928.4 ns |    17.59 ns |    13.73 ns |  33,660 |
| Wy2Hash64         |    941.5 ns |    20.63 ns |    14.92 ns |  33,192 |
| Rapid3HashMicro64 |    975.4 ns |    11.70 ns |     8.46 ns |  32,038 |
| Wy4Hash64         |    977.4 ns |    51.71 ns |    40.37 ns |  31,973 |
| ClHash64          |  1,031.2 ns |    62.35 ns |    48.68 ns |  30,304 |
| CityHashCrc128    |  1,280.8 ns |    19.66 ns |    15.35 ns |  24,399 |
| CityHashCrc256    |  1,383.1 ns |   100.37 ns |    78.37 ns |  22,594 |
| FarmHash64        |  1,545.7 ns |   101.21 ns |    79.01 ns |  20,217 |
| T1ha2Hash64       |  1,595.8 ns |    38.04 ns |    27.51 ns |  19,583 |
| CityHash128       |  1,664.3 ns |    32.00 ns |    23.14 ns |  18,777 |
| FarmHash128       |  1,696.6 ns |    28.68 ns |    20.74 ns |  18,419 |
| XxHash64          |  1,758.2 ns |    24.66 ns |    19.25 ns |  17,774 |
| CityHash64        |  1,780.9 ns |   179.09 ns |   139.82 ns |  17,547 |
| FarshHash32       |  1,923.6 ns |    28.35 ns |    20.50 ns |  16,246 |
| HighwayHash64     |  1,945.2 ns |    25.89 ns |    20.21 ns |  16,065 |
| Polymur2Hash64    |  2,580.6 ns |    30.68 ns |    23.95 ns |  12,110 |
| FarshHash64       |  2,802.7 ns |   134.81 ns |    97.47 ns |  11,150 |
| XxHash32          |  3,570.9 ns |    88.40 ns |    58.47 ns |   8,751 |
| Murmur3Hash128    |  3,923.7 ns |    37.84 ns |    25.03 ns |   7,964 |
| CityHash32        |  4,094.2 ns |    59.55 ns |    46.49 ns |   7,633 |
| FarmHash32        |  4,098.5 ns |    53.86 ns |    42.05 ns |   7,625 |
| Djb2Hash64        |  8,828.4 ns |   262.60 ns |   173.69 ns |   3,540 |
| MarvinHash32      |  8,876.1 ns |    89.23 ns |    64.52 ns |   3,521 |
| MarvinHash64      |  9,015.4 ns |   126.43 ns |    91.42 ns |   3,466 |
| Murmur3Hash32     |  9,257.5 ns |    73.97 ns |    57.75 ns |   3,376 |
| SipHash64         | 10,378.7 ns |    85.59 ns |    66.83 ns |   3,011 |
| SuperFastHash32   | 10,563.8 ns |    60.02 ns |    46.86 ns |   2,958 |
| Djb2Hash32        | 12,341.9 ns |    98.56 ns |    76.95 ns |   2,532 |
| SHA256            | 14,638.9 ns |    71.21 ns |    55.60 ns |   2,135 |
| Djb2AltHash32     | 21,015.4 ns |   313.50 ns |   244.76 ns |   1,487 |
| Djb2AltHash64     | 21,381.1 ns |   176.68 ns |   137.94 ns |   1,462 |
| Fnv1aHash32       | 28,141.7 ns |   155.20 ns |   112.22 ns |   1,110 |
| Fnv1aHash64       | 28,317.6 ns |   155.73 ns |   121.58 ns |   1,104 |
| SHA1              | 36,771.7 ns | 1,450.00 ns | 1,132.06 ns |     850 |
| MD5               | 48,091.6 ns |   176.00 ns |   127.26 ns |     650 |

#### Index functions

Four-byte integer inputs for 32-bit index APIs and eight-byte integer inputs for 64-bit and 128-bit index APIs.

| Method            | Input bytes |      Mean |     Error |    StdDev |
|-------------------|------------:|----------:|----------:|----------:|
| XxHash32          |           4 | 0.1430 ns | 0.1131 ns | 0.0883 ns |
| Fnv1aHash32       |           4 | 0.1858 ns | 0.0254 ns | 0.0184 ns |
| AesniHash64       |           8 | 0.1995 ns | 0.0533 ns | 0.0416 ns |
| SuperFastHash32   |           4 | 0.2021 ns | 0.0365 ns | 0.0285 ns |
| AesniHash128      |           8 | 0.2112 ns | 0.0154 ns | 0.0102 ns |
| Djb2AltHash32     |           4 | 0.2186 ns | 0.0588 ns | 0.0425 ns |
| Djb2Hash32        |           4 | 0.2230 ns | 0.0209 ns | 0.0151 ns |
| FoldHash64        |           8 | 0.2291 ns | 0.0476 ns | 0.0371 ns |
| FarmHash64        |           8 | 0.2317 ns | 0.0267 ns | 0.0193 ns |
| Gx2Hash64         |           8 | 0.2464 ns | 0.0884 ns | 0.0585 ns |
| FoldHashQuality64 |           8 | 0.2690 ns | 0.1242 ns | 0.0898 ns |
| CityHash64        |           8 | 0.3073 ns | 0.1444 ns | 0.1128 ns |
| Wy3Hash64         |           8 | 0.3075 ns | 0.0950 ns | 0.0742 ns |
| Wy2Hash64         |           8 | 0.3103 ns | 0.0979 ns | 0.0708 ns |
| MarvinHash32      |           4 | 0.3507 ns | 0.0373 ns | 0.0291 ns |
| Xx3Hash128        |           8 | 0.3793 ns | 0.1297 ns | 0.0938 ns |
| Murmur3Hash32     |           4 | 0.3850 ns | 0.1821 ns | 0.1422 ns |
| Gx2Hash32         |           4 | 0.4259 ns | 0.0246 ns | 0.0192 ns |
| Xx3Hash64         |           8 | 0.4422 ns | 0.0126 ns | 0.0091 ns |
| Rapid3HashMicro64 |           8 | 0.4546 ns | 0.0359 ns | 0.0238 ns |
| XxHash64          |           8 | 0.4640 ns | 0.0425 ns | 0.0332 ns |
| Wy4Hash64         |           8 | 0.4665 ns | 0.0238 ns | 0.0158 ns |
| Rapid3Hash64      |           8 | 0.4763 ns | 0.0284 ns | 0.0188 ns |
| T1ha2Hash64       |           8 | 0.4902 ns | 0.0682 ns | 0.0532 ns |
| Rapid3HashNano64  |           8 | 0.5175 ns | 0.1061 ns | 0.0767 ns |
| Murmur3Hash128    |           8 | 0.5250 ns | 0.0643 ns | 0.0465 ns |
| FarshHash32       |           4 | 0.5252 ns | 0.0195 ns | 0.0129 ns |
| Gx2Hash128        |           8 | 0.6791 ns | 0.0143 ns | 0.0301 ns |
| Djb2AltHash64     |           8 | 0.9047 ns | 0.0828 ns | 0.0599 ns |
| Djb2Hash64        |           8 | 0.9446 ns | 0.0667 ns | 0.0483 ns |
| MarvinHash64      |           8 | 1.1306 ns | 0.0139 ns | 0.0108 ns |
| Fnv1aHash64       |           8 | 1.1479 ns | 0.2026 ns | 0.1582 ns |
| FarmHash32        |           4 | 1.3253 ns | 0.1403 ns | 0.1095 ns |
| CityHash32        |           4 | 1.3927 ns | 0.1455 ns | 0.1052 ns |
| Polymur2Hash64    |           8 | 1.6204 ns | 0.1196 ns | 0.0791 ns |
| FarshHash64       |           8 | 2.3540 ns | 0.1411 ns | 0.0933 ns |
| FarmHash128       |           8 | 2.5364 ns | 0.0588 ns | 0.0459 ns |
| CityHash128       |           8 | 2.6198 ns | 0.0965 ns | 0.0753 ns |
| MeowHash128       |           8 | 5.9714 ns | 0.1011 ns | 0.0669 ns |
| MeowHash64        |           8 | 6.4924 ns | 0.1237 ns | 0.0895 ns |
| SipHash64         |           8 | 6.5482 ns | 0.2767 ns | 0.2160 ns |
| HighwayHash64     |           8 | 6.6784 ns | 0.1103 ns | 0.0861 ns |
| PolyXorHash128    |           8 | 8.9050 ns | 0.5310 ns | 0.3512 ns |

#### Mixer functions

Measured with input 42.

| Method | spec                             |     Mean |     Error |    StdDev |
|--------|----------------------------------|---------:|----------:|----------:|
| Mix64  | JM_xmx_Depth9_64                 | 1.258 ns | 0.0152 ns | 0.0118 ns |
| Mix64  | EZ_xmx_FastHash_64               | 1.265 ns | 0.0285 ns | 0.0223 ns |
| Mix64  | JM_mxm_Depth8_64                 | 1.281 ns | 0.0177 ns | 0.0138 ns |
| Mix64  | YC_xmx_XXH3_64                   | 1.289 ns | 0.0303 ns | 0.0236 ns |
| Mix64  | JM_mrm_Depth7_64                 | 1.290 ns | 0.0283 ns | 0.0221 ns |
| Mix64  | PK_rlxrlx_Umash_64               | 1.295 ns | 0.0161 ns | 0.0125 ns |
| Mix64  | DS_xmxmx_Mix05_64                | 1.442 ns | 0.0289 ns | 0.0191 ns |
| Mix64  | JM_mxmx_Depth11_64               | 1.482 ns | 0.0218 ns | 0.0170 ns |
| Mix64  | PE_rrxrrxmxxmxx_Nasam_64         | 1.485 ns | 0.0274 ns | 0.0214 ns |
| Mix64  | JM_xmrx_Depth12_64               | 1.486 ns | 0.0211 ns | 0.0164 ns |
| Mix64  | JM_xmxmxmx_Mx3_64                | 1.488 ns | 0.0162 ns | 0.0117 ns |
| Mix64  | DS_xmxmx_Mix02_64                | 1.489 ns | 0.0276 ns | 0.0183 ns |
| Mix64  | PE_rrxrrmrrxrrxmx_rrxmrrxmsx0_64 | 1.496 ns | 0.0207 ns | 0.0162 ns |
| Mix64  | JM_mxma_Depth11_64               | 1.497 ns | 0.0188 ns | 0.0147 ns |
| Mix64  | TE_rlxrlmrlxrlx_AxMix_64         | 1.499 ns | 0.0087 ns | 0.0057 ns |
| Mix64  | JM_mxmxm_Depth13_64              | 1.500 ns | 0.0213 ns | 0.0154 ns |
| Mix64  | DS_xmxmx_Mix04_64                | 1.501 ns | 0.0202 ns | 0.0146 ns |
| Mix64  | DL_xmxmx_Lea_64                  | 1.503 ns | 0.0295 ns | 0.0230 ns |
| Mix64  | JM_mxrmx_Depth14_64              | 1.505 ns | 0.0274 ns | 0.0214 ns |
| Mix64  | DS_xmxmx_Mix14_64                | 1.505 ns | 0.0346 ns | 0.0229 ns |
| Mix64  | DS_xmxmx_Mix03_64                | 1.508 ns | 0.0273 ns | 0.0213 ns |
| Mix64  | PE_rrxrrxmxmx_rrmxmx_64          | 1.509 ns | 0.0211 ns | 0.0165 ns |
| Mix64  | PE_xmxmx_Moremur_64              | 1.511 ns | 0.0166 ns | 0.0130 ns |
| Mix64  | WF_amx_Wymix_64                  | 1.511 ns | 0.0247 ns | 0.0193 ns |
| Mix64  | YC_xmxmx_XXH_64                  | 1.513 ns | 0.0320 ns | 0.0250 ns |
| Mix64  | WF_amxmx_Wymix_64                | 1.515 ns | 0.0276 ns | 0.0215 ns |
| Mix64  | DS_xmxmx_Mix06_64                | 1.516 ns | 0.0420 ns | 0.0303 ns |
| Mix64  | DS_xmxmx_Mix09_64                | 1.517 ns | 0.0252 ns | 0.0197 ns |
| Mix64  | DS_xmxmx_Mix13_64                | 1.518 ns | 0.0326 ns | 0.0255 ns |
| Mix64  | JM_xmxmx_Mx2_64                  | 1.524 ns | 0.0503 ns | 0.0364 ns |
| Mix64  | DS_xmxmx_Mix01_64                | 1.524 ns | 0.0458 ns | 0.0331 ns |
| Mix64  | DS_xmxmx_Mix07_64                | 1.525 ns | 0.0278 ns | 0.0217 ns |
| Mix64  | DS_xmxmx_Mix12_64                | 1.533 ns | 0.0569 ns | 0.0411 ns |
| Mix64  | DS_xmxmx_Mix10_64                | 1.534 ns | 0.0323 ns | 0.0252 ns |
| Mix64  | GP_mxxmxxm_CityHash_64           | 1.535 ns | 0.0320 ns | 0.0231 ns |
| Mix64  | DS_xmxmx_Mix11_64                | 1.541 ns | 0.0531 ns | 0.0415 ns |
| Mix64  | DS_xmxmx_Mix08_64                | 1.543 ns | 0.0476 ns | 0.0372 ns |
| Mix64  | DE_xmxmx_Degski_64               | 1.545 ns | 0.0484 ns | 0.0378 ns |
| Mix64  | AA_xmxmx_Murmur_64               | 1.574 ns | 0.0263 ns | 0.0190 ns |
| Mix32  | FP_xsxxmx_Fp64_32                | 1.578 ns | 0.0661 ns | 0.0516 ns |
| Mix32  | YC_xmxmx_XXH_32                  | 1.598 ns | 0.0307 ns | 0.0222 ns |
| Mix32  | DE_xmxmx_Degski_32               | 1.623 ns | 0.0388 ns | 0.0303 ns |
| Mix32  | CW_xmxmx_LowBias_32              | 1.641 ns | 0.0338 ns | 0.0244 ns |
| Mix32  | CW_xmxmxmx_Triple_32             | 1.647 ns | 0.0957 ns | 0.0747 ns |
| Mix64  | JM_mxmxmx_Depth15_64             | 1.720 ns | 0.0302 ns | 0.0236 ns |
| Mix32  | AA_xmxmx_Murmur_32               | 1.721 ns | 0.0140 ns | 0.0093 ns |