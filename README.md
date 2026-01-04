# FastHash - High-performance non-cryptographic hashes

[![NuGet](https://img.shields.io/nuget/v/Genbox.FastHash.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Genbox.FastHash/)

### Features

* API supports [ReadOnlySpan](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1) for optimal performance and flexibility
* Unsafe API that takes [byte*](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code) as input. Use this when performance is important.
* Most of the hashes are verified with test vectors from the original author
* High-performance zero-allocation implementations
* Index variants of each hash function that give the same output as if using the hash function, but takes in a 32bit/64bit integer.

### Hashes

These hash functions are included in the library.

| Name                                                                                                                                                    | Version | Authors                                                         |   License    |
|---------------------------------------------------------------------------------------------------------------------------------------------------------|:-------:|-----------------------------------------------------------------|:------------:|
| [AesniHash](https://github.com/synopse/mORMot2)                                                                                                         |    -    | Arnaud Bouchez                                                  | MPL/GPL/LGPL |
| [CityHash](https://github.com/google/cityhash)                                                                                                          |   1.1   | Geoff Pike, Jyrki Alakuijala                                    |     MIT      |
| [DJBHash](http://www.cse.yorku.ca/~oz/hash.html)                                                                                                        |    -    | Daniel J. Bernstein                                             |     None     |
| [FarmHash](https://github.com/google/farmhash)                                                                                                          |   1.1   | Geoff Pike                                                      |     MIT      |
| [FarshHash](https://github.com/Bulat-Ziganshin/FARSH)                                                                                                   |    -    | Bulat Ziganshin                                                 |     MIT      |
| [FNVHash](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)                                                                   |    -    | Glenn Fowler, Landon Curt Noll, Kiem-Phong Vo                   |     None     |
| [FoldHash](https://github.com/orlp/foldhash)                                                                                                            |  0.2.0  | Orson Peters                                                    |     Zlib     |
| [GxHash](https://github.com/ogxd/gxhash)                                                                                                                |    1    | Orso G.                                                         |     MIT      |
| [Gx2Hash](https://github.com/ogxd/gxhash)                                                                                                               |    2    | Orso G.                                                         |     MIT      |
| [HighwayHash](https://github.com/google/highwayhash)                                                                                                    |    -    | Jyrki Alakuijala, Bill Cox, Jan Wassenberg                      |  Apache 2.0  |
| [MarvinHash](https://github.com/dotnet/runtime/blob/4017327955f1d8ddc43980eb1848c52fbb131dfc/src/libraries/System.Private.CoreLib/src/System/Marvin.cs) |    -    | Niels Ferguson, Reid Borsuk, Jeffrey Cooperstein, Matthew Ellis |     MIT      |
| [MeowHash](https://github.com/cmuratori/meow_hash)                                                                                                      |    -    | Molly Rocket, Inc.                                              |     Zlib     |
| [MurmurHash](https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp)                                                                      |   3.0   | Austin Appleby                                                  |     None     |
| [PolymurHash](https://github.com/orlp/polymur-hash)                                                                                                     |   2.0   | Orson Peters                                                    |     Zlib     |
| [RapidHash](https://github.com/Nicoshev/rapidhash)                                                                                                      |    3    | Nicolas De Carli                                                |     MIT      |
| [SipHash](https://github.com/veorq/SipHash)                                                                                                             |   1.0   | Jean-Philippe Aumasson, Daniel J. Bernstein                     |   CC0 1.0    |
| [SuperFastHash](http://www.azillionmonkeys.com/qed/hash.html)                                                                                           |    -    | Paul Hsieh                                                      |   LGPL 2.1   |
| [WyHash](https://github.com/wangyi-fudan/wyhash)                                                                                                        | final 3 | Wang Yi                                                         |     None     |
| [xxHash](https://github.com/Cyan4973/xxHash)                                                                                                            |  0.8.1  | Yann Collet                                                     | BSD 2-Clause |

### Implementation status

The table below gives an overview of the implementations.

| Name          | Managed | Unsafe | 32bit | 64bit | 128bit | Index | Seeded | Secret | Verified |
|---------------|:-------:|:------:|:-----:|:-----:|:------:|:-----:|:------:|:------:|:--------:|
| AesniHash     |    x    |        |       |   x   |   x    |   x   |   x    |        |          |
| CityHash      |    x    |   x    |   x   |   x   |   x    |   x   |   x    |        |    x     |
| DJBHash       |    x    |   x    |   x   |   x   |        |   x   |        |        |          |
| FarmHash      |    x    |   x    |   x   |   x   |        |   x   |   x    |        |    -     |
| FarshHash     |    x    |   x    |       |   x   |        |   x   |   x    |        |    x     |
| FNVHash       |    x    |   x    |   x   |   x   |        |   x   |        |        |          |
| FoldHash      |    x    |        |       |   x   |        |   x   |   x    |   x    |          |
| GxHash        |    x    |        |   x   |   x   |   x    |   x   |   x    |        |          |
| Gx2Hash       |    x    |        |   x   |   x   |   x    |   x   |   x    |        |          |
| HighwayHash   |         |   x    |       |   x   |        |   x   |   x    |        |    x     |
| MarvinHash    |    x    |        |   x   |       |        |   x   |   x    |        |          |
| MeowHash      |         |   x    |       |   x   |   x    |   x   |        |        |          |
| MurmurHash    |    x    |   x    |   x   |       |   x    |   x   |   x    |        |    -     |
| PolymurHash   |    x    |        |       |   x   |        |   x   |   x    |        |          |
| RapidHash     |    x    |        |       |   x   |        |   x   |   x    |        |    x     |
| SipHash       |    x    |   x    |       |   x   |        |   x   |   x    |        |    x     |
| SuperFastHash |    x    |   x    |       |       |        |   x   |        |        |          |
| WyHash        |    x    |   x    |       |   x   |        |   x   |        |        |    x     |
| xxHash2       |    x    |   x    |   x   |   x   |        |   x   |   x    |        |    x     |
| xxHash3       |    x    |   x    |       |   x   |   x    |   x   |   x    |        |    x     |

* **Managed:** The there is a fully managed implementation in C#.
* **Unsafe:** There is an unmanaged implementation that uses pointers etc. These are usually faster.
* **Bits:** 32bit means there is a 32bit optimized implementation that returns an uint. 64bit means optimized for 64bit platforms.
* **Index:**  It has an _index_ version, which can hash a 32/64bit integer directly. Usually used for [Hash Table](https://en.wikipedia.org/wiki/Hash_table) mapping.
* **Seeded:** It takes an input seed which can help prevent denial-of-service due to hash collisions.
* **Secret:** It supports a user-provided secret. Much like seeded version it protects against DoS attacks, but with stronger security guarantees.
* **Verified:** The original author has provided test vectors and they have been tested against the implementation. A '-' means test vectors
  exist, but not yet implemented.

### Performance

#### Hash functions

Measured on 32 MB data. The unsafe versions are implemented using C# unsafe code.

**Note:** Speed is not everything. The quality of a hash is just as important, but much more difficult to measure. [See SMHasher](https://github.com/rurban/smhasher) for more
details on hash quality.

| Method                |        Mean |     Error |    StdDev |   MiB/s |
|-----------------------|------------:|----------:|----------:|--------:|
| GxHash128             |    266.9 ns |   0.25 ns |   0.15 ns | 117,083 |
| GxHash32              |    268.9 ns |   0.13 ns |   0.07 ns | 116,235 |
| GxHash64              |    268.9 ns |   0.16 ns |   0.09 ns | 116,222 |
| Gx2Hash32             |    273.3 ns |   2.00 ns |   1.32 ns | 114,346 |
| Gx2Hash128            |    273.8 ns |   2.78 ns |   1.84 ns | 114,123 |
| Gx2Hash64             |    274.4 ns |   5.37 ns |   3.88 ns | 113,885 |
| MeowHash128Unsafe     |    530.1 ns |   0.37 ns |   0.19 ns |  58,953 |
| MeowHash64Unsafe      |    531.5 ns |   2.39 ns |   1.58 ns |  58,799 |
| FoldHashQuality64     |    785.1 ns |   0.95 ns |   0.50 ns |  39,805 |
| FoldHash64            |    787.6 ns |   3.62 ns |   2.39 ns |  39,680 |
| Wy3Hash64Unsafe       |    888.4 ns |   2.24 ns |   1.48 ns |  35,176 |
| AesniHash64           |    956.9 ns |   9.70 ns |   6.41 ns |  32,656 |
| AesniHash128          |    957.9 ns |  10.05 ns |   5.98 ns |  32,624 |
| RapidHashMicro64      |  1,023.7 ns |   3.61 ns |   2.39 ns |  30,527 |
| RapidHashNano64       |  1,027.4 ns |   2.32 ns |   1.22 ns |  30,415 |
| RapidHash64           |  1,034.5 ns |  15.90 ns |   9.46 ns |  30,208 |
| Wy3Hash64             |  1,113.5 ns |   7.36 ns |   4.38 ns |  28,065 |
| FarmHash64Unsafe      |  1,264.9 ns |   2.77 ns |   1.65 ns |  24,706 |
| FarmHash64            |  1,265.0 ns |   1.02 ns |   0.53 ns |  24,704 |
| CityHash128           |  1,554.8 ns |   5.24 ns |   2.74 ns |  20,099 |
| CityHash128Unsafe     |  1,560.2 ns |   8.06 ns |   5.33 ns |  20,029 |
| CityHash64Unsafe      |  1,573.0 ns |   1.81 ns |   1.08 ns |  19,866 |
| CityHash64            |  1,622.3 ns |  31.70 ns |  32.56 ns |  19,262 |
| Xx2Hash64Unsafe       |  1,697.1 ns |  13.65 ns |   9.03 ns |  18,414 |
| Xx2Hash64             |  1,718.7 ns |  33.71 ns |  26.32 ns |  18,182 |
| FarshHash64Unsafe     |  2,370.7 ns |  23.88 ns |  15.79 ns |  13,182 |
| FarshHash64           |  2,927.0 ns |  21.70 ns |  12.91 ns |  10,677 |
| Xx2Hash32Unsafe       |  3,392.1 ns |  26.25 ns |  15.62 ns |   9,212 |
| Xx2Hash32             |  3,395.9 ns |  29.93 ns |  19.80 ns |   9,202 |
| Murmur3Hash128Unsafe  |  3,713.3 ns |   2.30 ns |   1.37 ns |   8,416 |
| Murmur3Hash128        |  3,804.6 ns |  14.37 ns |   9.51 ns |   8,214 |
| CityHash32Unsafe      |  3,917.9 ns |  10.12 ns |   6.02 ns |   7,976 |
| FarmHash32            |  3,942.2 ns |  10.61 ns |   7.01 ns |   7,927 |
| CityHash32            |  3,956.8 ns |  16.46 ns |   9.80 ns |   7,898 |
| FarmHash32Unsafe      |  3,964.4 ns |  42.80 ns |  25.47 ns |   7,883 |
| Xx3Hash64Unsafe       |  4,109.6 ns |  16.47 ns |   9.80 ns |   7,604 |
| Xx3Hash128Unsafe      |  4,112.9 ns |  25.21 ns |  13.19 ns |   7,598 |
| Polymur2Hash64        |  4,159.1 ns |   8.17 ns |   4.86 ns |   7,514 |
| MarvinHash64          |  8,552.2 ns |  38.51 ns |  25.47 ns |   3,654 |
| MarvinHash32          |  8,586.4 ns |  60.45 ns |  39.98 ns |   3,639 |
| Murmur3Hash32Unsafe   |  8,808.0 ns |   8.33 ns |   4.96 ns |   3,548 |
| Murmur3Hash32         |  8,822.5 ns |  15.04 ns |   7.87 ns |   3,542 |
| SuperFastHash32Unsafe | 10,088.7 ns |  25.39 ns |  15.11 ns |   3,098 |
| SuperFastHash32       | 10,092.9 ns |  27.41 ns |  18.13 ns |   3,096 |
| SipHash64             | 10,201.6 ns |  41.62 ns |  21.77 ns |   3,063 |
| SipHash64Unsafe       | 10,377.6 ns |   2.38 ns |   1.25 ns |   3,011 |
| Djb2Hash32Unsafe      | 20,187.0 ns |   8.72 ns |   4.56 ns |   1,548 |
| Djb2Hash64            | 20,187.2 ns |  19.96 ns |  13.20 ns |   1,548 |
| Djb2Hash32            | 20,195.8 ns |  33.28 ns |  22.01 ns |   1,547 |
| Djb2Hash64Unsafe      | 20,197.7 ns |  26.61 ns |  15.84 ns |   1,547 |
| Fnv1aHash32           | 26,892.0 ns |  22.45 ns |  14.85 ns |   1,162 |
| Fnv1aHash32Unsafe     | 26,893.7 ns |  20.96 ns |  10.96 ns |   1,162 |
| Fnv1aHash64           | 26,918.9 ns |  55.92 ns |  33.27 ns |   1,161 |
| Fnv1aHash64Unsafe     | 27,149.7 ns | 247.35 ns | 147.20 ns |   1,151 |

#### Index functions

| Method              |       Mean |     Error |    StdDev |
|---------------------|-----------:|----------:|----------:|
| Gx2Hash64           |  0.0796 ns | 0.0198 ns | 0.0175 ns |
| Fnv1aHash32         |  0.1131 ns | 0.0102 ns | 0.0068 ns |
| GxHash64            |  0.1132 ns | 0.0171 ns | 0.0113 ns |
| Xx2Hash32           |  0.1249 ns | 0.0141 ns | 0.0093 ns |
| FarmHash64          |  0.1299 ns | 0.0069 ns | 0.0041 ns |
| FoldHash64          |  0.1302 ns | 0.0157 ns | 0.0104 ns |
| GxHash32            |  0.1316 ns | 0.0205 ns | 0.0136 ns |
| AesniHash64         |  0.1352 ns | 0.0083 ns | 0.0055 ns |
| Murmur3Hash32       |  0.1373 ns | 0.0267 ns | 0.0209 ns |
| Djb2Hash32          |  0.1886 ns | 0.0065 ns | 0.0034 ns |
| Gx2Hash32           |  0.1997 ns | 0.0149 ns | 0.0078 ns |
| CityHash64          |  0.3420 ns | 0.0210 ns | 0.0125 ns |
| Wy3Hash64           |  0.3537 ns | 0.0048 ns | 0.0029 ns |
| Xx3Hash64           |  0.3700 ns | 0.0167 ns | 0.0099 ns |
| SuperFastHash32     |  0.3980 ns | 0.0073 ns | 0.0044 ns |
| FoldHashQuality64   |  0.4183 ns | 0.0319 ns | 0.0231 ns |
| RapidHashNano64     |  0.4331 ns | 0.0283 ns | 0.0221 ns |
| RapidHashMicro64    |  0.4446 ns | 0.0278 ns | 0.0184 ns |
| Xx2Hash64           |  0.4563 ns | 0.0137 ns | 0.0091 ns |
| RapidHash64         |  0.4869 ns | 0.0556 ns | 0.0617 ns |
| MarvinHash32        |  0.6195 ns | 0.0053 ns | 0.0032 ns |
| FarshHash64         |  0.8928 ns | 0.0050 ns | 0.0033 ns |
| Fnv1aHash64         |  1.2018 ns | 0.0386 ns | 0.0230 ns |
| Djb2Hash64          |  1.3681 ns | 0.0443 ns | 0.0293 ns |
| MarvinHash64        |  1.3807 ns | 0.0078 ns | 0.0047 ns |
| FarmHash32          |  1.4742 ns | 0.0076 ns | 0.0040 ns |
| CityHash32          |  1.5003 ns | 0.0066 ns | 0.0040 ns |
| Polymur2Hash64      |  1.8500 ns | 0.0177 ns | 0.0093 ns |
| SipHash64           |  7.0921 ns | 0.0919 ns | 0.0608 ns |
| MeowHash64Unsafe    | 39.4472 ns | 0.7643 ns | 0.6382 ns |
| HighwayHash64Unsafe | 81.4736 ns | 1.5304 ns | 1.4315 ns |

#### Mixer functions

| Method | spec                             |     Mean |     Error |    StdDev |
|--------|----------------------------------|---------:|----------:|----------:|
| Mix64  | JM_xmrx_Depth12_64               | 1.136 ns | 0.0187 ns | 0.0111 ns |
| Mix64  | DS_xmxmx_Mix01_64                | 1.159 ns | 0.0104 ns | 0.0062 ns |
| Mix64  | YC_xmx_XXH3_64                   | 1.167 ns | 0.0177 ns | 0.0105 ns |
| Mix64  | DS_xmxmx_Mix09_64                | 1.168 ns | 0.0115 ns | 0.0076 ns |
| Mix64  | DS_xmxmx_Mix11_64                | 1.169 ns | 0.0023 ns | 0.0014 ns |
| Mix64  | DS_xmxmx_Mix05_64                | 1.172 ns | 0.0252 ns | 0.0150 ns |
| Mix64  | EZ_xmx_FastHash_64               | 1.172 ns | 0.0130 ns | 0.0086 ns |
| Mix64  | JM_mxrmx_Depth14_64              | 1.179 ns | 0.0191 ns | 0.0126 ns |
| Mix64  | JM_mrm_Depth7_64                 | 1.180 ns | 0.0151 ns | 0.0100 ns |
| Mix64  | JM_xmx_Depth9_64                 | 1.183 ns | 0.0357 ns | 0.0236 ns |
| Mix64  | JM_mxm_Depth8_64                 | 1.184 ns | 0.0136 ns | 0.0090 ns |
| Mix64  | PE_xmxmx_Moremur_64              | 1.205 ns | 0.0335 ns | 0.0222 ns |
| Mix64  | JM_xmxmx_Mx2_64                  | 1.351 ns | 0.0121 ns | 0.0072 ns |
| Mix64  | JM_mxmx_Depth11_64               | 1.357 ns | 0.0105 ns | 0.0063 ns |
| Mix64  | DL_xmxmx_Lea_64                  | 1.369 ns | 0.0059 ns | 0.0035 ns |
| Mix64  | PE_rrxrrxmxxmxx_Nasam_64         | 1.372 ns | 0.0165 ns | 0.0109 ns |
| Mix64  | DS_xmxmx_Mix12_64                | 1.375 ns | 0.0288 ns | 0.0190 ns |
| Mix64  | JM_mxmxmx_Depth15_64             | 1.376 ns | 0.0347 ns | 0.0230 ns |
| Mix64  | DE_xmxmx_Degski_64               | 1.378 ns | 0.0265 ns | 0.0175 ns |
| Mix64  | TE_rlxrlmrlxrlx_AxMix_64         | 1.379 ns | 0.0106 ns | 0.0070 ns |
| Mix64  | AA_xmxmx_Murmur_64               | 1.382 ns | 0.0280 ns | 0.0185 ns |
| Mix64  | DS_xmxmx_Mix03_64                | 1.385 ns | 0.0203 ns | 0.0106 ns |
| Mix64  | WF_amxmx_Wymix_64                | 1.386 ns | 0.0141 ns | 0.0093 ns |
| Mix64  | DS_xmxmx_Mix08_64                | 1.387 ns | 0.0184 ns | 0.0121 ns |
| Mix64  | WF_amx_Wymix_64                  | 1.389 ns | 0.0204 ns | 0.0135 ns |
| Mix64  | DS_xmxmx_Mix10_64                | 1.389 ns | 0.0324 ns | 0.0234 ns |
| Mix64  | DS_xmxmx_Mix02_64                | 1.390 ns | 0.0483 ns | 0.0377 ns |
| Mix64  | PE_rrxrrxmxmx_rrmxmx_64          | 1.391 ns | 0.0219 ns | 0.0145 ns |
| Mix64  | DS_xmxmx_Mix07_64                | 1.392 ns | 0.0329 ns | 0.0196 ns |
| Mix64  | PK_rlxrlx_Umash_64               | 1.392 ns | 0.0194 ns | 0.0128 ns |
| Mix64  | DS_xmxmx_Mix04_64                | 1.394 ns | 0.0207 ns | 0.0137 ns |
| Mix64  | PE_rrxrrmrrxrrxmx_rrxmrrxmsx0_64 | 1.396 ns | 0.0445 ns | 0.0295 ns |
| Mix64  | JM_mxma_Depth11_64               | 1.399 ns | 0.0296 ns | 0.0196 ns |
| Mix64  | JM_mxmxm_Depth13_64              | 1.411 ns | 0.0425 ns | 0.0253 ns |
| Mix64  | DS_xmxmx_Mix14_64                | 1.413 ns | 0.0507 ns | 0.0335 ns |
| Mix64  | YC_xmxmx_XXH2_64                 | 1.415 ns | 0.0331 ns | 0.0219 ns |
| Mix64  | DS_xmxmx_Mix06_64                | 1.425 ns | 0.0382 ns | 0.0227 ns |
| Mix64  | DS_xmxmx_Mix13_64                | 1.454 ns | 0.0522 ns | 0.0580 ns |
| Mix64  | JM_xmxmxmx_Mx3_64                | 1.569 ns | 0.0120 ns | 0.0063 ns |
| Mix64  | GP_mxxmxxm_CityHash_64           | 1.591 ns | 0.0262 ns | 0.0174 ns |
| Mix32  | CW_xmxmxmx_Triple_32             | 1.947 ns | 0.0484 ns | 0.0320 ns |
| Mix32  | AA_xmxmx_Murmur_32               | 2.045 ns | 0.0407 ns | 0.0242 ns |
| Mix32  | DE_xmxmx_Degski_32               | 2.203 ns | 0.0160 ns | 0.0095 ns |
| Mix32  | YC_xmxmx_XXH2_32                 | 2.222 ns | 0.0138 ns | 0.0092 ns |
| Mix32  | FP_xsxxmx_Fp64_32                | 2.234 ns | 0.0140 ns | 0.0093 ns |
| Mix32  | CW_xmxmx_LowBias_32              | 2.255 ns | 0.0656 ns | 0.0702 ns |
