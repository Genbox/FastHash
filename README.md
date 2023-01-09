# FastHash - High-performance non-cryptographic hashes

[![NuGet](https://img.shields.io/nuget/v/Genbox.FastHash.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Genbox.FastHash/)

### Features

* API supports [ReadOnlySpan](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1) for optimal performance and flexibility
* Unsafe API that takes [byte*](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code) as input. Use this when performance is important.
* Most of the hashes are verified with test vectors from the original author
* High-performance zero-allocation implementations

### Hashes

These hash functions are included in the library.

| Name                                                                                                                                                    | Version | Authors                                                         |   License    |
|---------------------------------------------------------------------------------------------------------------------------------------------------------|:-------:|-----------------------------------------------------------------|:------------:|
| [CityHash](https://github.com/google/cityhash)                                                                                                          |   1.1   | Geoff Pike, Jyrki Alakuijala                                    |     MIT      |
| [DJBHash](http://www.cse.yorku.ca/~oz/hash.html)                                                                                                        |    -    | Daniel J. Bernstein                                             |     None     |
| [FarmHash](https://github.com/google/farmhash)                                                                                                          |   1.1   | Geoff Pike                                                      |     MIT      |
| [FarshHash](https://github.com/Bulat-Ziganshin/FARSH)                                                                                                   |    -    | Bulat Ziganshin                                                 |     MIT      |
| [FNVHash](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)                                                                   |    -    | Glenn Fowler, Landon Curt Noll, Kiem-Phong Vo                   |     None     |
| [HighwayHash](https://github.com/google/highwayhash)                                                                                                    |    -    | Jyrki Alakuijala, Bill Cox, Jan Wassenberg                      |  Apache 2.0  |
| [MarvinHash](https://github.com/dotnet/runtime/blob/4017327955f1d8ddc43980eb1848c52fbb131dfc/src/libraries/System.Private.CoreLib/src/System/Marvin.cs) |    -    | Niels Ferguson, Reid Borsuk, Jeffrey Cooperstein, Matthew Ellis |     MIT      |
| [MurmurHash](https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp)                                                                      |   3.0   | Austin Appleby                                                  |     None     |
| [SipHash](https://github.com/veorq/SipHash)                                                                                                             |   1.0   | Jean-Philippe Aumasson, Daniel J. Bernstein                     |   CC0 1.0    |
| [SuperFastHash](http://www.azillionmonkeys.com/qed/hash.html)                                                                                           |    -    | Paul Hsieh                                                      |   LGPL 2.1   |
| [WyHash](https://github.com/wangyi-fudan/wyhash)                                                                                                        | final 3 | Wang Yi                                                         |     None     |
| [xxHash](https://github.com/Cyan4973/xxHash)                                                                                                            |  0.8.1  | Yann Collet                                                     | BSD 2-Clause |

### Implementation status

The table below gives an overview of the implementations.

* **Managed**: Means the there is a fully managed implementation in C#.
* **Unsafe**: There is an unmanaged implementation that uses pointers etc. These are usually faster.
* **Bits**: "32bit" means there is a 32bit optimized implementation that returns an uint. 64bit means optimized for 64bit platforms.
* **Index**: If there is an "index" version, which can hash a 32/64bit integer directly. Usually used for [Hash Table](https://en.wikipedia.org/wiki/Hash_table) mapping.
* **Seeded**: The implementation takes an input seed which can help prevent denial-of-service due to hash collisions.
* **Secret**: Supports a user-provided secret. Much like seeded version it protects against DoS attacks, but with stronger security guarantees.
* **Verified**: Means the original author has provided test vectors and they have been tested against the implementation. A '-' means test vectors
  exist, but not yet implemented.

| Name          | Managed | Unsafe | 32bit | 64bit | 128bit | Index | Seeded | Secret | Verified |
|---------------|:-------:|:------:|:-----:|:-----:|:------:|:-----:|:------:|:------:|:--------:|
| CityHash      |    x    |   x    |   x   |   x   |   x    |   x   |   x    |        |    x     |
| DJBHash       |    x    |   x    |   x   |   x   |        |   x   |        |        |          |
| FarmHash      |    x    |   x    |   x   |   x   |        |   x   |   x    |        |    -     |
| FarshHash     |    x    |   x    |       |   x   |        |       |   x    |        |    x     |
| FNVHash       |    x    |   x    |   x   |   x   |        |   x   |        |        |          |
| HighwayHash   |         |   x    |       |   x   |        |       |   x    |        |    x     |
| MarvinHash    |    x    |        |   x   |       |        |   x   |   x    |        |          |
| MurmurHash    |    x    |   x    |   x   |       |   x    |   x   |   x    |        |    -     |
| SipHash       |    x    |   x    |       |   x   |        |       |   x    |        |    x     |
| SuperFastHash |    x    |   x    |       |       |        |   x   |        |        |          |
| WyHash        |    x    |   x    |       |   x   |        |   x   |        |        |    x     |
| xxHash2       |    x    |   x    |   x   |   x   |        |   x   |   x    |        |    x     |
| xxHash3       |    x    |   x    |       |   x   |   x    |   x   |   x    |        |    x     |

### Performance

Measured on 32 MB data. The unsafe versions are implemented using C# unsafe code.

**Note:** Speed is not everything. The quality of a hash is just as important, but much more difficult to measure. [See SMHasher](https://github.com/rurban/smhasher) for more
details on hash quality.

|               Hash method | MB/s    |
|--------------------------:|---------|
|       Wy3Hash64UnsafeTest | 	15,909 |
|             Wy3Hash64Test | 	15,256 |
|       Xx2Hash64UnsafeTest | 	13,374 |
|      Xx3Hash128UnsafeTest | 	12,582 |
|     CityHash128UnsafeTest | 	12,558 |
|             Xx2Hash64Test | 	12,438 |
|      CityHash64UnsafeTest | 	12,424 |
|       Xx3Hash64UnsafeTest | 	12,418 |
|           CityHash128Test | 	10,630 |
|            CityHash64Test | 	9,047  |
|       Xx2Hash32UnsafeTest | 	8,128  |
|            FarmHash64Test | 	8,089  |
|      FarmHash64UnsafeTest | 	7,987  |
|        Murmur3Hash128Test | 	7,982  |
|  Murmur3Hash128UnsafeTest | 	7,356  |
|             Xx2Hash32Test | 	7,287  |
|      FarmHash32UnsafeTest | 	7,233  |
|     FarshHash64UnsafeTest | 	7,186  |
|            FarmHash32Test | 	6,621  |
|      CityHash32UnsafeTest | 	6,433  |
|            CityHash32Test | 	5,712  |
|         Murmur3Hash32Test | 	3,796  |
|   Murmur3Hash32UnsafeTest | 	3,760  |
|           FarshHash64Test | 	3,474  |
|       SuperFastHash32Test | 	3,304  |
| SuperFastHash32UnsafeTest | 	3,259  |
|          MarvinHash32Test | 	3,135  |
|       SipHash64UnsafeTest | 	2,581  |
|             SipHash64Test | 	2,576  |
|            Djb2Hash32Test | 	1,451  |
|            Djb2Hash64Test | 	1,448  |
|      Djb2Hash64UnsafeTest | 	1,447  |
|      Djb2Hash32UnsafeTest | 	1,444  |
|           Fnv1aHash32Test | 	1,092  |
|     Fnv1aHash32UnsafeTest | 	1,092  |
|     Fnv1aHash64UnsafeTest | 	1,092  |
|           Fnv1aHash64Test | 	1,079  |