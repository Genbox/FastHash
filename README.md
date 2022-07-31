# FastHash - High-performance non-cryptographic hashes

[![NuGet](https://img.shields.io/nuget/v/Genbox.FastHash.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Genbox.FastHash/)

### Hashes
These hash functions are included in the library.

| Name                                                                                                                                                    | Authors                                                         | Managed | Unsafe | 32bit | 64bit | 128bit | 256bit |
|---------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------|:-------:|:------:|:-----:|:-----:|:------:|:------:|
| [CityHash](https://github.com/google/cityhash)                                                                                                          | Geoff Pike, Jyrki Alakuijala                                    |         |   x    |   x   |   x   |   x    |   x    |
| DJBHash                                                                                                                                                 | Daniel J. Bernstein                                             |    x    |   x    |   x   |       |        |        |
| [FarmHash](https://github.com/google/farmhash)                                                                                                          | Geoff Pike                                                      |    x    |   x    |   x   |   x   |        |        |
| [FarshHash](https://github.com/Bulat-Ziganshin/FARSH)                                                                                                   | Bulat Ziganshin                                                 |    x    |   x    |       |   x   |        |        |
| [FNVHash](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)                                                                   | Glenn Fowler, Landon Curt Noll, Kiem-Phong Vo                   |    x    |   x    |   x   |   x   |        |        |
| [HighwayHash](https://github.com/google/highwayhash)                                                                                                    | Jyrki Alakuijala, Bill Cox, Jan Wassenberg                      |         |   x    |       |   x   |        |        |
| [MarvinHash](https://github.com/dotnet/runtime/blob/4017327955f1d8ddc43980eb1848c52fbb131dfc/src/libraries/System.Private.CoreLib/src/System/Marvin.cs) | Niels Ferguson, Reid Borsuk, Jeffrey Cooperstein, Matthew Ellis |    x    |        |   x   |       |        |        |
| [MurmurHash](https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp)                                                                      | Austin Appleby                                                  |    x    |   x    |   x   |       |   x    |        |
| [SipHash](https://github.com/veorq/SipHash)                                                                                                             | Jean-Philippe Aumasson, Daniel J. Bernstein                     |    x    |   x    |       |   x   |        |        |
| [SuperFastHash](http://www.azillionmonkeys.com/qed/hash.html)                                                                                           | Paul Hsieh                                                      |    x    |   x    |   x   |       |        |        |
| [WyHash](https://github.com/wangyi-fudan/wyhash)                                                                                                        | Wang Yi                                                         |    x    |   x    |       |   x   |        |        |
| [xxHash](https://github.com/Cyan4973/xxHash)                                                                                                            | Yann Collet                                                     |    x    |   x    |   x   |   x   |   x    |        |

### Performance
Measured on 32 MB data. The unsafe versions are implemented using C# unsafe code.

**Note:** Speed is not everything. The quality of a hash is just as important, but much more difficult to measure. [See SMHasher](https://github.com/rurban/smhasher) for more details on hash quality.

|           Hash method | MB/s   |
|----------------------:|--------|
|   Fnv1aYtHash32Unsafe | 19,926 |
|       Wy3Hash64Unsafe | 15,721 |
|       Xx2Hash64Unsafe | 12,813 |
|             Wy3Hash64 | 11,572 |
|      Xx3Hash128Unsafe | 11,083 |
|       Xx3Hash64Unsafe | 11,057 |
|             Xx2Hash64 | 10,590 |
|      FarmHash64Unsafe | 7,970  |
|       Xx2Hash32Unsafe | 7,929  |
|            FarmHash64 | 7,739  |
|  Murmur3Hash128Unsafe | 7,152  |
|      FarmHash32Unsafe | 6,997  |
|     FarshHash64Unsafe | 6,969  |
|        Murmur3Hash128 | 6,473  |
|             Xx2Hash32 | 5,551  |
|            FarmHash32 | 5,037  |
|   Murmur3Hash32Unsafe | 3,746  |
|         Murmur3Hash32 | 3,524  |
| SuperFastHash32Unsafe | 3,236  |
|          MarvinHash32 | 3,120  |
|           FarshHash64 | 2,998  |
|       SuperFastHash32 | 2,875  |
|       SipHash64Unsafe | 2,545  |
|             SipHash64 | 2,315  |
|            Djb2Hash32 | 1,437  |
|      Djb2Hash32Unsafe | 1,433  |
|   HighwayHash64Unsafe | 1,312  |
|           Fnv1aHash32 | 1,090  |
|           Fnv1aHash64 | 1,089  |
|     Fnv1aHash64Unsafe | 1,087  |
|     Fnv1aHash32Unsafe | 1,070  |
