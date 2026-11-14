using System.Runtime.Intrinsics.Arm;
using Genbox.FastHash.AbslHash;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.ClHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.FoldHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.HighwayHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.PolymurHash;
using Genbox.FastHash.PolyXorHash;
using Genbox.FastHash.RapidHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.T1haHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;
using Aes = System.Runtime.Intrinsics.X86.Aes;

namespace Genbox.FastHash.TestShared;

public static unsafe class AlgorithmCatalog
{
    public static readonly Hash32Algorithm[] Hash32Algorithms =
    [
        new Hash32Algorithm(nameof(CityHash32), static data => CityHash32.ComputeHash(data), static (data, length) => CityHash32Unsafe.ComputeHash(data, length), 0xbf7a812aU),
        new Hash32Algorithm(nameof(Djb2AltHash32), static data => Djb2AltHash32.ComputeHash(data), static (data, length) => Djb2AltHash32Unsafe.ComputeHash(data, length), 0x3614edceU),
        new Hash32Algorithm(nameof(Djb2Hash32), static data => Djb2Hash32.ComputeHash(data), static (data, length) => Djb2Hash32Unsafe.ComputeHash(data, length), 0x753d179cU),
        new Hash32Algorithm(nameof(FarmHash32), static data => FarmHash32.ComputeHash(data), static (data, length) => FarmHash32Unsafe.ComputeHash(data, length), 0xbf7a812aU),
        new Hash32Algorithm(nameof(FarshHash32), static data => FarshHash32.ComputeHash(data), static (data, length) => FarshHash32Unsafe.ComputeHash(data, length), 0xb2682d81U),
        new Hash32Algorithm(nameof(Fnv1aHash32), static data => Fnv1aHash32.ComputeHash(data), static (data, length) => Fnv1aHash32Unsafe.ComputeHash(data, length), 0x23e07ef6U),
        new Hash32Algorithm(nameof(Gx2Hash32), static data => Gx2Hash32.ComputeHash(data), null, 0x2ab73ac3U),
        new Hash32Algorithm(nameof(MarvinHash32), static data => MarvinHash32.ComputeHash(data, 0x2A, 0x2B), null, 0x22bf28adU),
        new Hash32Algorithm(nameof(Murmur3Hash32), static data => Murmur3Hash32.ComputeHash(data), static (data, length) => Murmur3Hash32Unsafe.ComputeHash(data, length), 0x877908f6U),
        new Hash32Algorithm(nameof(SuperFastHash32), static data => SuperFastHash32.ComputeHash(data), static (data, length) => SuperFastHash32Unsafe.ComputeHash(data, length), 0xb241e85eU),
        new Hash32Algorithm(nameof(XxHash32), static data => XxHash32.ComputeHash(data), static (data, length) => XxHash32Unsafe.ComputeHash(data, length), 0x94c7c62bU)
    ];

    public static readonly Hash64Algorithm[] Hash64Algorithms =
    [
        new Hash64Algorithm(nameof(AbslHash64), static data => AbslHash64.ComputeHash(data), static (data, length) => AbslHash64Unsafe.ComputeHash(data, length), 0x8b2bbb1b4062cf77UL),
        new Hash64Algorithm(nameof(AesniHash64), static data => AesniHash64.ComputeHash(data), null, 0xf63e2b26874a1c28UL),
        new Hash64Algorithm(nameof(CityHash64), static data => CityHash64.ComputeHash(data), static (data, length) => CityHash64Unsafe.ComputeHash(data, length), 0x3ae1e13a9834ec17UL),
        new Hash64Algorithm(nameof(ClHash64), static data => ClHash64.ComputeHash(data), static (data, length) => ClHash64Unsafe.ComputeHash(data, length), 0xbf0643cf9fcda4b6UL),
        new Hash64Algorithm(nameof(Djb2AltHash64), static data => Djb2AltHash64.ComputeHash(data), static (data, length) => Djb2AltHash64Unsafe.ComputeHash(data, length), 0x8a6c33f93614edceUL),
        new Hash64Algorithm(nameof(Djb2Hash64), static data => Djb2Hash64.ComputeHash(data), static (data, length) => Djb2Hash64Unsafe.ComputeHash(data, length), 0xe6335a9753d179cUL),
        new Hash64Algorithm(nameof(FarmHash64), static data => FarmHash64.ComputeHash(data), static (data, length) => FarmHash64Unsafe.ComputeHash(data, length), 0x3ae1e13a9834ec17UL),
        new Hash64Algorithm(nameof(FarshHash64), static data => FarshHash64.ComputeHash(data), static (data, length) => FarshHash64Unsafe.ComputeHash(data, length), 0x5be17e4b2682d81UL),
        new Hash64Algorithm(nameof(Fnv1aHash64), static data => Fnv1aHash64.ComputeHash(data), static (data, length) => Fnv1aHash64Unsafe.ComputeHash(data, length), 0x472f08108ee035d6UL),
        new Hash64Algorithm(nameof(FoldHash64), static data => FoldHash64.ComputeHash(data), null, 0x89a7683c357c30a5UL),
        new Hash64Algorithm(nameof(FoldHashQuality64), static data => FoldHashQuality64.ComputeHash(data), null, 0x4a682c6f70baadf9UL),
        new Hash64Algorithm(nameof(Gx2Hash64), static data => Gx2Hash64.ComputeHash(data), null, 0xb5ebcc792ab73ac3UL),
        new Hash64Algorithm(nameof(HighwayHash64), static data => HighwayHash64.ComputeHash(data), static (data, length) => HighwayHash64Unsafe.ComputeHash(data, length), 0x16cf3706478e6c63UL),
        new Hash64Algorithm(nameof(MarvinHash64), static data => MarvinHash64.ComputeHash(data), null, 0xda1867a0d9d24457UL),
        new Hash64Algorithm(nameof(MeowHash64), static data => MeowHash64.ComputeHash(data), static (data, length) => MeowHash64Unsafe.ComputeHash(data, length), 0x9e2ec1bdabccf5ceUL),
        new Hash64Algorithm(nameof(Polymur2Hash64), static data => Polymur2Hash64.ComputeHash(data), null, 0x6953803cdbe54bd8UL),
        new Hash64Algorithm(nameof(Rapid3Hash64), static data => Rapid3Hash64.ComputeHash(data), null, 0x6ad5c20e8c50f49fUL),
        new Hash64Algorithm(nameof(Rapid3HashMicro64), static data => Rapid3HashMicro64.ComputeHash(data), null, 0x6ad5c20e8c50f49fUL),
        new Hash64Algorithm(nameof(Rapid3HashNano64), static data => Rapid3HashNano64.ComputeHash(data), null, 0x6ad5c20e8c50f49fUL),
        new Hash64Algorithm(nameof(SipHash64), static data => SipHash64.ComputeHash(data), static (data, length) => SipHash64Unsafe.ComputeHash(data, length), 0x9722637e422efdbaUL),
        new Hash64Algorithm(nameof(T1ha2Hash64), static data => T1ha2Hash64.ComputeHash(data), null, 0xd7291b0ea7f087c6UL),
#if WYHASH_CONDOM
        new(nameof(Wy2Hash64), static data => Wy2Hash64.ComputeHash(data), static (data, length) => Wy2Hash64Unsafe.ComputeHash(data, length), 0xdea9212d26be9e18UL),
        new(nameof(Wy3Hash64), static data => Wy3Hash64.ComputeHash(data), static (data, length) => Wy3Hash64Unsafe.ComputeHash(data, length), 0x597dcf7548bb0cf1UL),
        new(nameof(Wy4Hash64), static data => Wy4Hash64.ComputeHash(data), static (data, length) => Wy4Hash64Unsafe.ComputeHash(data, length), 0x720588a1f3ee41abUL),
#else
        new Hash64Algorithm(nameof(Wy2Hash64), static data => Wy2Hash64.ComputeHash(data), static (data, length) => Wy2Hash64Unsafe.ComputeHash(data, length), 0x76803f3ba4789996UL),
        new Hash64Algorithm(nameof(Wy3Hash64), static data => Wy3Hash64.ComputeHash(data), static (data, length) => Wy3Hash64Unsafe.ComputeHash(data, length), 0xc25274572a72a23fUL),
        new Hash64Algorithm(nameof(Wy4Hash64), static data => Wy4Hash64.ComputeHash(data), static (data, length) => Wy4Hash64Unsafe.ComputeHash(data, length), 0x409c0abe436fe9b5UL),
#endif
        new Hash64Algorithm(nameof(XxHash64), static data => XxHash64.ComputeHash(data), static (data, length) => XxHash64Unsafe.ComputeHash(data, length), 0xdebb823cafa8e475UL),
        new Hash64Algorithm(nameof(Xx3Hash64), static data => Xx3Hash64.ComputeHash(data), static (data, length) => Xx3Hash64Unsafe.ComputeHash(data, length), 0xc33bf4b7b1ff39bfUL)
    ];

    public static readonly Hash128Algorithm[] Hash128Algorithms =
    [
        new Hash128Algorithm(nameof(AesniHash128), static data => AesniHash128.ComputeHash(data), null, new UInt128(0x2fb2afedbe2353f6UL, 0xf63e2b26874a1c28UL)),
        new Hash128Algorithm(nameof(CityHash128), static data => CityHash128.ComputeHash(data), static (data, length) => CityHash128Unsafe.ComputeHash(data, length), new UInt128(0x61eafa2daf63dc35UL, 0x6bf0a8aa9d91fe18UL)),
        new Hash128Algorithm(nameof(CityHashCrc128), static data => CityHashCrc128.ComputeHash(data), static (data, length) => CityHashCrc128Unsafe.ComputeHash(data, length), null),
        new Hash128Algorithm(nameof(FarmHash128), static data => FarmHash128.ComputeHash(data), static (data, length) => FarmHash128Unsafe.ComputeHash(data, length), new UInt128(0x61eafa2daf63dc35UL, 0x6bf0a8aa9d91fe18UL)),
        new Hash128Algorithm(nameof(Gx2Hash128), static data => Gx2Hash128.ComputeHash(data), null, new UInt128(0xe4db2dd14b69b987UL, 0xb5ebcc792ab73ac3UL)),
        new Hash128Algorithm(nameof(MeowHash128), static data => MeowHash128.ComputeHash(data), static (data, length) => MeowHash128Unsafe.ComputeHash(data, length), new UInt128(0xaf4e6840c517dee7UL, 0x9e2ec1bdabccf5ceUL)),
        new Hash128Algorithm(nameof(Murmur3Hash128), static data => Murmur3Hash128.ComputeHash(data), static (data, length) => Murmur3Hash128Unsafe.ComputeHash(data, length), new UInt128(0x67bf53b7fd393d08UL, 0x89738414b7d4d679UL)),
        new Hash128Algorithm(nameof(PolyXorHash128), static data => PolyXorHash128.ComputeHash(data), static (data, length) => PolyXorHash128Unsafe.ComputeHash(data, length), new UInt128(0x398e2557b5b2b0f1UL, 0x43986d0679d26651UL)),
        new Hash128Algorithm(nameof(Xx3Hash128), static data => Xx3Hash128.ComputeHash(data), static (data, length) => Xx3Hash128Unsafe.ComputeHash(data, length), new UInt128(0x132f9b355c6cacdfUL, 0xc06f090f147cd76aUL))
    ];

    public static readonly Hash256Algorithm[] Hash256Algorithms =
    [
        new Hash256Algorithm(nameof(CityHashCrc256), static (data, result) => CityHashCrc256.ComputeHash(data, result), static (data, length, result) => CityHashCrc256Unsafe.ComputeHash(data, length, result))
    ];

    public static readonly Index32Algorithm[] Index32Algorithms =
    [
        new Index32Algorithm(nameof(CityHash32), CityHash32.ComputeIndex, CityHash32.ComputeHash),
        new Index32Algorithm(nameof(Djb2AltHash32), Djb2AltHash32.ComputeIndex, Djb2AltHash32.ComputeHash),
        new Index32Algorithm(nameof(Djb2Hash32), Djb2Hash32.ComputeIndex, Djb2Hash32.ComputeHash),
        new Index32Algorithm(nameof(FarmHash32), FarmHash32.ComputeIndex, FarmHash32.ComputeHash),
        new Index32Algorithm(nameof(FarshHash32), FarshHash32.ComputeIndex, FarshHash32.ComputeHash),
        new Index32Algorithm(nameof(Fnv1aHash32), Fnv1aHash32.ComputeIndex, Fnv1aHash32.ComputeHash),
        new Index32Algorithm(nameof(Gx2Hash32), static input => Gx2Hash32.ComputeIndex(input), static data => Gx2Hash32.ComputeHash(data)),
        new Index32Algorithm(nameof(MarvinHash32), MarvinHash32.ComputeIndex, static data => MarvinHash32.ComputeHash(data)),
        new Index32Algorithm(nameof(Murmur3Hash32), Murmur3Hash32.ComputeIndex, static data => Murmur3Hash32.ComputeHash(data)),
        new Index32Algorithm(nameof(SuperFastHash32), SuperFastHash32.ComputeIndex, SuperFastHash32.ComputeHash),
        new Index32Algorithm(nameof(XxHash32), XxHash32.ComputeIndex, static data => XxHash32.ComputeHash(data))
    ];

    public static readonly Index64Algorithm[] Index64Algorithms =
    [
        new Index64Algorithm(nameof(AesniHash64), static input => AesniHash64.ComputeIndex(input), static data => AesniHash64.ComputeHash(data)),
        new Index64Algorithm(nameof(CityHash64), CityHash64.ComputeIndex, CityHash64.ComputeHash),
        new Index64Algorithm(nameof(Djb2AltHash64), Djb2AltHash64.ComputeIndex, Djb2AltHash64.ComputeHash),
        new Index64Algorithm(nameof(Djb2Hash64), Djb2Hash64.ComputeIndex, Djb2Hash64.ComputeHash),
        new Index64Algorithm(nameof(FarmHash64), FarmHash64.ComputeIndex, static data => FarmHash64.ComputeHash(data)),
        new Index64Algorithm(nameof(FarshHash64), FarshHash64.ComputeIndex, FarshHash64.ComputeHash),
        new Index64Algorithm(nameof(FoldHash64), FoldHash64.ComputeIndex, FoldHash64.ComputeHash),
        new Index64Algorithm(nameof(FoldHashQuality64), FoldHashQuality64.ComputeIndex, FoldHashQuality64.ComputeHash),
        new Index64Algorithm(nameof(Fnv1aHash64), Fnv1aHash64.ComputeIndex, Fnv1aHash64.ComputeHash),
        new Index64Algorithm(nameof(Gx2Hash64), static input => Gx2Hash64.ComputeIndex(input), static data => Gx2Hash64.ComputeHash(data)),
        new Index64Algorithm(nameof(HighwayHash64), HighwayHash64.ComputeIndex, HighwayHash64.ComputeHash),
        new Index64Algorithm(nameof(MarvinHash64), MarvinHash64.ComputeIndex, static data => MarvinHash64.ComputeHash(data)),
        new Index64Algorithm(nameof(MeowHash64), MeowHash64.ComputeIndex, MeowHash64.ComputeHash),
        new Index64Algorithm(nameof(Polymur2Hash64), Polymur2Hash64.ComputeIndex, static data => Polymur2Hash64.ComputeHash(data)),
        new Index64Algorithm(nameof(Rapid3Hash64), static input => Rapid3Hash64.ComputeIndex(input), static data => Rapid3Hash64.ComputeHash(data)),
        new Index64Algorithm(nameof(Rapid3HashMicro64), static input => Rapid3HashMicro64.ComputeIndex(input), static data => Rapid3HashMicro64.ComputeHash(data)),
        new Index64Algorithm(nameof(Rapid3HashNano64), static input => Rapid3HashNano64.ComputeIndex(input), static data => Rapid3HashNano64.ComputeHash(data)),
        new Index64Algorithm(nameof(SipHash64), static input => SipHash64.ComputeIndex(input), static data => SipHash64.ComputeHash(data)),
        new Index64Algorithm(nameof(Wy2Hash64), Wy2Hash64.ComputeIndex, static data => Wy2Hash64.ComputeHash(data)),
        new Index64Algorithm(nameof(Wy3Hash64), Wy3Hash64.ComputeIndex, static data => Wy3Hash64.ComputeHash(data)),
        new Index64Algorithm(nameof(Wy4Hash64), static input => Wy4Hash64.ComputeIndex(input), static data => Wy4Hash64.ComputeHash(data)),
        new Index64Algorithm(nameof(XxHash64), XxHash64.ComputeIndex, static data => XxHash64.ComputeHash(data)),
        new Index64Algorithm(nameof(Xx3Hash64), static input => Xx3Hash64.ComputeIndex(input), static data => Xx3Hash64.ComputeHash(data))
    ];

    public static readonly Index128Algorithm[] Index128Algorithms =
    [
        new Index128Algorithm(nameof(AesniHash128), static input => AesniHash128.ComputeIndex(input), static data => AesniHash128.ComputeHash(data)),
        new Index128Algorithm(nameof(CityHash128), static input => CityHash128.ComputeIndex(input), CityHash128.ComputeHash),
        new Index128Algorithm(nameof(FarmHash128), static input => FarmHash128.ComputeIndex(input), FarmHash128.ComputeHash),
        new Index128Algorithm(nameof(Gx2Hash128), static input => Gx2Hash128.ComputeIndex(input), static data => Gx2Hash128.ComputeHash(data)),
        new Index128Algorithm(nameof(MeowHash128), MeowHash128.ComputeIndex, MeowHash128.ComputeHash),
        new Index128Algorithm(nameof(Murmur3Hash128), static input => Murmur3Hash128.ComputeIndex(input), static data => Murmur3Hash128.ComputeHash(data)),
        new Index128Algorithm(nameof(PolyXorHash128), static input => PolyXorHash128.ComputeIndex(input), static data => PolyXorHash128.ComputeHash(data)),
        new Index128Algorithm(nameof(Xx3Hash128), static input => Xx3Hash128.ComputeIndex(input), static data => Xx3Hash128.ComputeHash(data))
    ];

    public static bool IsSupported(string name) => name switch
    {
        nameof(AesniHash64) or nameof(AesniHash128) => AesniHash128.IsSupported,
        nameof(CityHashCrc128) or nameof(CityHashCrc256) => CityHashCrc256.IsSupported,
        nameof(ClHash64) => ClHash64.IsSupported,
        nameof(Gx2Hash32) or nameof(Gx2Hash64) or nameof(Gx2Hash128) =>
            Aes.IsSupported ||
            (System.Runtime.Intrinsics.Arm.Aes.IsSupported && AdvSimd.IsSupported),
        nameof(MeowHash64) or nameof(MeowHash128) => MeowHash128.IsSupported,
        nameof(PolyXorHash128) => PolyXorHash128.IsSupported,
        _ => true
    };

    public static Hash32Algorithm GetHash32(string name) => Hash32Algorithms.Single(x => x.Name == name);
    public static Hash64Algorithm GetHash64(string name) => Hash64Algorithms.Single(x => x.Name == name);
    public static Hash128Algorithm GetHash128(string name) => Hash128Algorithms.Single(x => x.Name == name);
    public static Index32Algorithm GetIndex32(string name) => Index32Algorithms.Single(x => x.Name == name);
    public static Index64Algorithm GetIndex64(string name) => Index64Algorithms.Single(x => x.Name == name);
    public static Index128Algorithm GetIndex128(string name) => Index128Algorithms.Single(x => x.Name == name);
}