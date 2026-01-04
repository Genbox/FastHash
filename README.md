# FastHash - High-performance non-cryptographic hashes

[![NuGet](https://img.shields.io/nuget/v/Genbox.FastHash.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Genbox.FastHash/)

### Features

* API supports [ReadOnlySpan](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1) for optimal performance and flexibility
* Unsafe API that takes [byte*](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code) as input. Use this when performance is important.
* Most of the hashes are verified with test vectors from the original author
* High-performance zero-allocation implementations
* Index variants of each hash function that give the same output as if using the hash function, but takes in a 32bit/64bit integer.
*

### Hashes

These hash functions are included in the library.

| Name                                                                                                                                                    | Version | Authors                                                         |   License    |
|---------------------------------------------------------------------------------------------------------------------------------------------------------|:-------:|-----------------------------------------------------------------|:------------:|
| [CityHash](https://github.com/google/cityhash)                                                                                                          |   1.1   | Geoff Pike, Jyrki Alakuijala                                    |     MIT      |
| [DJBHash](http://www.cse.yorku.ca/~oz/hash.html)                                                                                                        |    -    | Daniel J. Bernstein                                             |     None     |
| [FarmHash](https://github.com/google/farmhash)                                                                                                          |   1.1   | Geoff Pike                                                      |     MIT      |
| [FarshHash](https://github.com/Bulat-Ziganshin/FARSH)                                                                                                   |    -    | Bulat Ziganshin                                                 |     MIT      |
| [FNVHash](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)                                                                   |    -    | Glenn Fowler, Landon Curt Noll, Kiem-Phong Vo                   |     None     |
| [FoldHash](https://github.com/orlp/foldhash)                                                                                                            |  0.2.0  | Orson Peters                                                    |     Zlib     |
| [HighwayHash](https://github.com/google/highwayhash)                                                                                                    |    -    | Jyrki Alakuijala, Bill Cox, Jan Wassenberg                      |  Apache 2.0  |
| [MarvinHash](https://github.com/dotnet/runtime/blob/4017327955f1d8ddc43980eb1848c52fbb131dfc/src/libraries/System.Private.CoreLib/src/System/Marvin.cs) |    -    | Niels Ferguson, Reid Borsuk, Jeffrey Cooperstein, Matthew Ellis |     MIT      |
| [MurmurHash](https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp)                                                                      |   3.0   | Austin Appleby                                                  |     None     |
| [RapidHash](https://github.com/Nicoshev/rapidhash)                                                                                                      |    3    | Nicolas De Carli                                                |     MIT      |
| [SipHash](https://github.com/veorq/SipHash)                                                                                                             |   1.0   | Jean-Philippe Aumasson, Daniel J. Bernstein                     |   CC0 1.0    |
| [SuperFastHash](http://www.azillionmonkeys.com/qed/hash.html)                                                                                           |    -    | Paul Hsieh                                                      |   LGPL 2.1   |
| [WyHash](https://github.com/wangyi-fudan/wyhash)                                                                                                        | final 3 | Wang Yi                                                         |     None     |
| [xxHash](https://github.com/Cyan4973/xxHash)                                                                                                            |  0.8.1  | Yann Collet                                                     | BSD 2-Clause |

### Implementation status

The table below gives an overview of the implementations.

| Name          | Managed | Unsafe | 32bit | 64bit | 128bit | Index | Seeded | Secret | Verified |
|---------------|:-------:|:------:|:-----:|:-----:|:------:|:-----:|:------:|:------:|:--------:|
| CityHash      |    x    |   x    |   x   |   x   |   x    |   x   |   x    |        |    x     |
| DJBHash       |    x    |   x    |   x   |   x   |        |   x   |        |        |          |
| FarmHash      |    x    |   x    |   x   |   x   |        |   x   |   x    |        |    -     |
| FarshHash     |    x    |   x    |       |   x   |        |       |   x    |        |    x     |
| FNVHash       |    x    |   x    |   x   |   x   |        |   x   |        |        |          |
| FoldHash      |    x    |        |       |   x   |        |       |   x    |   x    |          |
| HighwayHash   |         |   x    |       |   x   |        |       |   x    |        |    x     |
| MarvinHash    |    x    |        |   x   |       |        |   x   |   x    |        |          |
| MurmurHash    |    x    |   x    |   x   |       |   x    |   x   |   x    |        |    -     |
| SipHash       |    x    |   x    |       |   x   |        |       |   x    |        |    x     |
| SuperFastHash |    x    |   x    |       |       |        |   x   |        |        |          |
| WyHash        |    x    |   x    |       |   x   |        |   x   |        |        |    x     |
| xxHash2       |    x    |   x    |   x   |   x   |        |   x   |   x    |        |    x     |
| xxHash3       |    x    |   x    |       |   x   |   x    |   x   |   x    |        |    x     |

* **Managed:** The there is a fully managed implementation in C#.
* **Unsafe:** There is an unmanaged implementation that uses pointers etc. These are usually faster.
* **Bits:** 32bit means there is a 32bit optimized implementation that returns an uint. 64bit means optimized for 64bit platforms.
* **Index:**  It has an _index_ version, which½ can hash a 32/64bit integer directly. Usually used for [Hash Table](https://en.wikipedia.org/wiki/Hash_table) mapping.
* **Seeded:** It takes an input seed which can help prevent denial-of-service due to hash collisions.
* **Secret:** It supports a user-provided secret. Much like seeded version it protects against DoS attacks, but with stronger security guarantees.
* **Verified:** The original author has provided test vectors and they have been tested against the implementation. A '-' means test vectors
  exist, but not yet implemented.

### Performance

#### Hash functions

Measured on 32 MB data. The unsafe versions are implemented using C# unsafe code.

**Note:** Speed is not everything. The quality of a hash is just as important, but much more difficult to measure. [See SMHasher](https://github.com/rurban/smhasher) for more
details on hash quality.

|           Hash method | MB/s    |
|----------------------:|---------|
|       Wy3Hash64Unsafe | 	15.909 |
|             Wy3Hash64 | 	15.256 |
|       Xx2Hash64Unsafe | 	13.374 |
|      Xx3Hash128Unsafe | 	12.582 |
|     CityHash128Unsafe | 	12.558 |
|             Xx2Hash64 | 	12.438 |
|      CityHash64Unsafe | 	12.424 |
|       Xx3Hash64Unsafe | 	12.418 |
|           CityHash128 | 	10.630 |
|            CityHash64 | 	9.047  |
|       Xx2Hash32Unsafe | 	8.128  |
|            FarmHash64 | 	8.089  |
|      FarmHash64Unsafe | 	7.987  |
|        Murmur3Hash128 | 	7.982  |
|  Murmur3Hash128Unsafe | 	7.356  |
|             Xx2Hash32 | 	7.287  |
|      FarmHash32Unsafe | 	7.233  |
|     FarshHash64Unsafe | 	7.186  |
|            FarmHash32 | 	6.621  |
|      CityHash32Unsafe | 	6.433  |
|            CityHash32 | 	5.712  |
|         Murmur3Hash32 | 	3.796  |
|   Murmur3Hash32Unsafe | 	3.760  |
|           FarshHash64 | 	3.474  |
|       SuperFastHash32 | 	3.304  |
| SuperFastHash32Unsafe | 	3.259  |
|          MarvinHash32 | 	3.135  |
|       SipHash64Unsafe | 	2.581  |
|             SipHash64 | 	2.576  |
|            Djb2Hash32 | 	1.451  |
|            Djb2Hash64 | 	1.448  |
|      Djb2Hash64Unsafe | 	1.447  |
|      Djb2Hash32Unsafe | 	1.444  |
|           Fnv1aHash32 | 	1.092  |
|     Fnv1aHash32Unsafe | 	1.092  |
|     Fnv1aHash64Unsafe | 	1.092  |
|           Fnv1aHash64 | 	1.079  |

#### Index functions

| Method          |       Mean |     Error |    StdDev |
|-----------------|-----------:|----------:|----------:|
| Fnv1aHash32     |  0.4503 ns | 0.0227 ns | 0.0213 ns |
| Xx2Hash32       |  0.5403 ns | 0.0155 ns | 0.0130 ns |
| Djb2Hash32      |  0.6525 ns | 0.0311 ns | 0.0275 ns |
| FarmHash64      |  0.6587 ns | 0.0354 ns | 0.0421 ns |
| Murmur3Hash32   |  0.6738 ns | 0.0143 ns | 0.0126 ns |
| Xx2Hash64       |  1.0247 ns | 0.0428 ns | 0.0379 ns |
| SuperFastHash32 |  1.0324 ns | 0.0479 ns | 0.0448 ns |
| MarvinHash32    |  1.1625 ns | 0.0214 ns | 0.0190 ns |
| CityHash64      |  1.2441 ns | 0.0555 ns | 0.0639 ns |
| Djb2Hash64      |  2.3256 ns | 0.0743 ns | 0.0966 ns |
| CityHash32      |  2.9076 ns | 0.0770 ns | 0.0682 ns |
| Fnv1aHash64     |  2.9311 ns | 0.0873 ns | 0.0934 ns |
| FarmHash32      |  2.9699 ns | 0.0828 ns | 0.0775 ns |
| Wy3Hash64       |  3.0273 ns | 0.0675 ns | 0.0632 ns |
| SipHash64       | 10.4360 ns | 0.1979 ns | 0.1851 ns |

#### Mixer functions

| Method              |      Mean |     Error |    StdDev |
|---------------------|----------:|----------:|----------:|
| FastHash_64         | 0.8871 ns | 0.0281 ns | 0.0249 ns |
| Murmur_32           | 0.8982 ns | 0.0343 ns | 0.0287 ns |
| Xmx_64              | 0.9057 ns | 0.0379 ns | 0.0336 ns |
| Murmur_32_Seed      | 0.9305 ns | 0.0229 ns | 0.0203 ns |
| XXH2_32_Seed        | 0.9482 ns | 0.0317 ns | 0.0281 ns |
| XXH2_32             | 0.9487 ns | 0.0470 ns | 0.0541 ns |
| MoreMur_64          | 0.9561 ns | 0.0389 ns | 0.0364 ns |
| Murmur_64           | 0.9569 ns | 0.0500 ns | 0.0614 ns |
| Murmur_32_SeedMix   | 1.0322 ns | 0.0507 ns | 0.1144 ns |
| XXH2_32_SeedMix     | 1.1050 ns | 0.0308 ns | 0.0273 ns |
| Murmur14_64         | 1.1320 ns | 0.0175 ns | 0.0163 ns |
| XXH2_64             | 1.1555 ns | 0.0212 ns | 0.0188 ns |
| Mx3_64              | 1.3364 ns | 0.0320 ns | 0.0284 ns |
| Xmx_64_Seed         | 1.3795 ns | 0.0468 ns | 0.0437 ns |
| Nasam_64            | 1.3889 ns | 0.0233 ns | 0.0207 ns |
| Murmur_64_Seed      | 1.4960 ns | 0.0507 ns | 0.0474 ns |
| Murmur14_64_SeedMix | 1.5502 ns | 0.0337 ns | 0.0282 ns |
| Xmx_64_SeedMix      | 1.5543 ns | 0.0249 ns | 0.0221 ns |
| FastHash_64_SeedMix | 1.5654 ns | 0.0413 ns | 0.0345 ns |
| XXH2_64_Seed        | 1.5730 ns | 0.0286 ns | 0.0254 ns |
| Murmur14_64_Seed    | 1.5763 ns | 0.0356 ns | 0.0315 ns |
| MoreMur_64_SeedMix  | 1.5887 ns | 0.0209 ns | 0.0174 ns |
| MoreMur_64_Seed     | 1.5952 ns | 0.0563 ns | 0.0526 ns |
| XXH2_64_SeedMix     | 1.6038 ns | 0.0280 ns | 0.0249 ns |
| Nasam_64_SeedMix    | 1.6203 ns | 0.0610 ns | 0.0509 ns |
| Nasam_64_Seed       | 1.6231 ns | 0.0477 ns | 0.0446 ns |
| Murmur_64_SeedMix   | 1.6244 ns | 0.0273 ns | 0.0242 ns |
| FastHash_64_Seed    | 1.6737 ns | 0.0615 ns | 0.0732 ns |
| City_64_Seed        | 1.6955 ns | 0.0625 ns | 0.1218 ns |
| Mx3_64_Seed         | 1.8109 ns | 0.0653 ns | 0.1092 ns |
| Mx3_64_SeedMix      | 1.8354 ns | 0.0483 ns | 0.0537 ns |
