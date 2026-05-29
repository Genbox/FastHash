using Genbox.FastHash.AbslHash;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
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
using Genbox.FastHash.RapidHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.T1haHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.TestShared;

public static unsafe class AlgorithmCatalog
{
    public static readonly Hash32Algorithm[] Hash32Algorithms =
    [
        new(nameof(CityHash32), static data => CityHash32.ComputeHash(data), static (data, length) => CityHash32Unsafe.ComputeHash(data, length), [0x2A, 0x81, 0x7A, 0xBF]),
        new(nameof(Djb2AltHash32), static data => Djb2AltHash32.ComputeHash(data), static (data, length) => Djb2AltHash32Unsafe.ComputeHash(data, length), [0xCE, 0xED, 0x14, 0x36]),
        new(nameof(Djb2Hash32), static data => Djb2Hash32.ComputeHash(data), static (data, length) => Djb2Hash32Unsafe.ComputeHash(data, length), [0x9C, 0x17, 0x3D, 0x75]),
        new(nameof(FarmHash32), static data => FarmHash32.ComputeHash(data), static (data, length) => FarmHash32Unsafe.ComputeHash(data, length), [0x2A, 0x81, 0x7A, 0xBF]),
        new(nameof(Fnv1aHash32), static data => Fnv1aHash32.ComputeHash(data), static (data, length) => Fnv1aHash32Unsafe.ComputeHash(data, length), [0xF6, 0x7E, 0xE0, 0x23]),
        new(nameof(Gx2Hash32), static data => Gx2Hash32.ComputeHash(data), null, [0xC3, 0x3A, 0xB7, 0x2A]),
        new(nameof(MarvinHash32), static data => MarvinHash32.ComputeHash(data, 0x2A, 0x2B), null, [0xAD, 0x28, 0xBF, 0x22]),
        new(nameof(Murmur3Hash32), static data => Murmur3Hash32.ComputeHash(data), static (data, length) => Murmur3Hash32Unsafe.ComputeHash(data, length), [0xF6, 0x08, 0x79, 0x87]),
        new(nameof(SuperFastHash32), static data => SuperFastHash32.ComputeHash(data), static (data, length) => SuperFastHash32Unsafe.ComputeHash(data, length), [0x5E, 0xE8, 0x41, 0xB2]),
        new(nameof(XxHash32), static data => XxHash32.ComputeHash(data), static (data, length) => XxHash32Unsafe.ComputeHash(data, length), [0x2B, 0xC6, 0xC7, 0x94])
    ];

    public static readonly Hash64Algorithm[] Hash64Algorithms =
    [
        new(nameof(AbslHash64), static data => AbslHash64.ComputeHash(data), null, [0x77, 0xCF, 0x62, 0x40, 0x1B, 0xBB, 0x2B, 0x8B]),
        new(nameof(AesniHash64), static data => AesniHash64.ComputeHash(data), null, [0x28, 0x1C, 0x4A, 0x87, 0x26, 0x2B, 0x3E, 0xF6]),
        new(nameof(CityHash64), static data => CityHash64.ComputeHash(data), static (data, length) => CityHash64Unsafe.ComputeHash(data, length), [0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A]),
        new(nameof(Djb2AltHash64), static data => Djb2AltHash64.ComputeHash(data), static (data, length) => Djb2AltHash64Unsafe.ComputeHash(data, length), [0xCE, 0xED, 0x14, 0x36, 0xF9, 0x33, 0x6C, 0x8A]),
        new(nameof(Djb2Hash64), static data => Djb2Hash64.ComputeHash(data), static (data, length) => Djb2Hash64Unsafe.ComputeHash(data, length), [0x9C, 0x17, 0x3D, 0x75, 0xA9, 0x35, 0x63, 0x0E]),
        new(nameof(FarshHash64), static data => FarshHash64.ComputeHash(data), static (data, length) => FarshHash64Unsafe.ComputeHash(data, length), [0x81, 0x2D, 0x68, 0xB2, 0xE4, 0x17, 0xBE, 0x05]),
        new(nameof(FarmHash64), static data => FarmHash64.ComputeHash(data), static (data, length) => FarmHash64Unsafe.ComputeHash(data, length), [0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A]),
        new(nameof(Fnv1aHash64), static data => Fnv1aHash64.ComputeHash(data), static (data, length) => Fnv1aHash64Unsafe.ComputeHash(data, length), [0xD6, 0x35, 0xE0, 0x8E, 0x10, 0x08, 0x2F, 0x47]),
        new(nameof(FoldHash64), static data => FoldHash64.ComputeHash(data), null, [0xA5, 0x30, 0x7C, 0x35, 0x3C, 0x68, 0xA7, 0x89]),
        new(nameof(FoldHashQuality64), static data => FoldHashQuality64.ComputeHash(data), null, [0xF9, 0xAD, 0xBA, 0x70, 0x6F, 0x2C, 0x68, 0x4A]),
        new(nameof(Gx2Hash64), static data => Gx2Hash64.ComputeHash(data), null, [0xC3, 0x3A, 0xB7, 0x2A, 0x79, 0xCC, 0xEB, 0xB5]),
        new(nameof(HighwayHash64Unsafe), null, static (data, length) => HighwayHash64Unsafe.ComputeHash(data, length), [0x63, 0x6C, 0x8E, 0x47, 0x06, 0x37, 0xCF, 0x16]),
        new(nameof(MarvinHash64), static data => MarvinHash64.ComputeHash(data), null, [0x57, 0x44, 0xD2, 0xD9, 0xA0, 0x67, 0x18, 0xDA]),
        new(nameof(MeowHash64Unsafe), null, static (data, length) => MeowHash64Unsafe.ComputeHash(data, length), [0xCE, 0xF5, 0xCC, 0xAB, 0xBD, 0xC1, 0x2E, 0x9E]),
        new(nameof(Polymur2Hash64), static data => Polymur2Hash64.ComputeHash(data), null, [0xD8, 0x4B, 0xE5, 0xDB, 0x3C, 0x80, 0x53, 0x69]),
        new(nameof(Rapid3Hash64), static data => Rapid3Hash64.ComputeHash(data), null, [0x9F, 0xF4, 0x50, 0x8C, 0x0E, 0xC2, 0xD5, 0x6A]),
        new(nameof(Rapid3HashMicro64), static data => Rapid3HashMicro64.ComputeHash(data), null, [0x9F, 0xF4, 0x50, 0x8C, 0x0E, 0xC2, 0xD5, 0x6A]),
        new(nameof(Rapid3HashNano64), static data => Rapid3HashNano64.ComputeHash(data), null, [0x9F, 0xF4, 0x50, 0x8C, 0x0E, 0xC2, 0xD5, 0x6A]),
        new(nameof(SipHash64), static data => SipHash64.ComputeHash(data), static (data, length) => SipHash64Unsafe.ComputeHash(data, length), [0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97]),
        new(nameof(T1ha2Hash64), static data => T1ha2Hash64.ComputeHash(data), null, [0xC6, 0x87, 0xF0, 0xA7, 0x0E, 0x1B, 0x29, 0xD7]),
        new(nameof(Wy3Hash64), static data => Wy3Hash64.ComputeHash(data), static (data, length) => Wy3Hash64Unsafe.ComputeHash(data, length), [0x3F, 0xA2, 0x72, 0x2A, 0x57, 0x74, 0x52, 0xC2]),
        new(nameof(Wy4Hash64), static data => Wy4Hash64.ComputeHash(data), static (data, length) => Wy4Hash64Unsafe.ComputeHash(data, length), [0xC5, 0x96, 0x5C, 0x8B, 0x33, 0x7D, 0x3F, 0x56]),
        new(nameof(XxHash64), static data => XxHash64.ComputeHash(data), static (data, length) => XxHash64Unsafe.ComputeHash(data, length), [0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE]),
        new(nameof(Xx3Hash64), static data => Xx3Hash64.ComputeHash(data), static (data, length) => Xx3Hash64Unsafe.ComputeHash(data, length), [0xBF, 0x39, 0xFF, 0xB1, 0xB7, 0xF4, 0x3B, 0xC3])
    ];

    public static readonly Hash128Algorithm[] Hash128Algorithms =
    [
        new(nameof(AesniHash128), static data => AesniHash128.ComputeHash(data), null, [0x28, 0x1C, 0x4A, 0x87, 0x26, 0x2B, 0x3E, 0xF6, 0xF6, 0x53, 0x23, 0xBE, 0xED, 0xAF, 0xB2, 0x2F]),
        new(nameof(CityHash128), static data => CityHash128.ComputeHash(data), static (data, length) => CityHash128Unsafe.ComputeHash(data, length), [0x18, 0xFE, 0x91, 0x9D, 0xAA, 0xA8, 0xF0, 0x6B, 0x35, 0xDC, 0x63, 0xAF, 0x2D, 0xFA, 0xEA, 0x61]),
        new(nameof(CityHashCrc128), static data => CityHashCrc128.ComputeHash(data), static (data, length) => CityHashCrc128Unsafe.ComputeHash(data, length), null),
        new(nameof(FarmHash128), static data => FarmHash128.ComputeHash(data), static (data, length) => FarmHash128Unsafe.ComputeHash(data, length), [0x18, 0xFE, 0x91, 0x9D, 0xAA, 0xA8, 0xF0, 0x6B, 0x35, 0xDC, 0x63, 0xAF, 0x2D, 0xFA, 0xEA, 0x61]),
        new(nameof(Gx2Hash128), static data => Gx2Hash128.ComputeHash(data), null, [0xC3, 0x3A, 0xB7, 0x2A, 0x79, 0xCC, 0xEB, 0xB5, 0x87, 0xB9, 0x69, 0x4B, 0xD1, 0x2D, 0xDB, 0xE4]),
        new(nameof(MeowHash128Unsafe), null, static (data, length) => MeowHash128Unsafe.ComputeHash(data, length), [0xCE, 0xF5, 0xCC, 0xAB, 0xBD, 0xC1, 0x2E, 0x9E, 0xE7, 0xDE, 0x17, 0xC5, 0x40, 0x68, 0x4E, 0xAF]),
        new(nameof(Murmur3Hash128), static data => Murmur3Hash128.ComputeHash(data), static (data, length) => Murmur3Hash128Unsafe.ComputeHash(data, length), [0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67]),
        new(nameof(Xx3Hash128), static data => Xx3Hash128.ComputeHash(data), static (data, length) => Xx3Hash128Unsafe.ComputeHash(data, length), [0x6A, 0xD7, 0x7C, 0x14, 0x0F, 0x09, 0x6F, 0xC0, 0xDF, 0xAC, 0x6C, 0x5C, 0x35, 0x9B, 0x2F, 0x13])
    ];

    public static readonly Hash256Algorithm[] Hash256Algorithms =
    [
        new(nameof(CityHashCrc256), static (data, result) => CityHashCrc256.ComputeHash(data, result), static (data, length, result) => CityHashCrc256Unsafe.ComputeHash(data, length, result))
    ];

    public static readonly Index32Algorithm[] Index32Algorithms =
    [
        new(nameof(CityHash32), CityHash32.ComputeIndex, CityHash32.ComputeHash),
        new(nameof(Djb2AltHash32), Djb2AltHash32.ComputeIndex, Djb2AltHash32.ComputeHash),
        new(nameof(Djb2Hash32), Djb2Hash32.ComputeIndex, Djb2Hash32.ComputeHash),
        new(nameof(FarmHash32), FarmHash32.ComputeIndex, FarmHash32.ComputeHash),
        new(nameof(Fnv1aHash32), Fnv1aHash32.ComputeIndex, Fnv1aHash32.ComputeHash),
        new(nameof(Gx2Hash32), static input => Gx2Hash32.ComputeIndex(input), static data => Gx2Hash32.ComputeHash(data)),
        new(nameof(MarvinHash32), MarvinHash32.ComputeIndex, static data => MarvinHash32.ComputeHash(data)),
        new(nameof(Murmur3Hash32), Murmur3Hash32.ComputeIndex, static data => Murmur3Hash32.ComputeHash(data)),
        new(nameof(SuperFastHash32), SuperFastHash32.ComputeIndex, SuperFastHash32.ComputeHash),
        new(nameof(XxHash32), XxHash32.ComputeIndex, static data => XxHash32.ComputeHash(data))
    ];

    public static readonly Index64Algorithm[] Index64Algorithms =
    [
        new(nameof(AesniHash64), static input => AesniHash64.ComputeIndex(input), static data => AesniHash64.ComputeHash(data)),
        new(nameof(CityHash64), CityHash64.ComputeIndex, CityHash64.ComputeHash),
        new(nameof(Djb2AltHash64), Djb2AltHash64.ComputeIndex, Djb2AltHash64.ComputeHash),
        new(nameof(Djb2Hash64), Djb2Hash64.ComputeIndex, Djb2Hash64.ComputeHash),
        new(nameof(FarshHash64), static input => FarshHash64.ComputeIndex(input), static data => FarshHash64.ComputeHash(data)),
        new(nameof(FoldHash64), static input => FoldHash64.ComputeIndex(input, 0), static data => FoldHash64.ComputeHash(data)),
        new(nameof(FoldHashQuality64), static input => FoldHashQuality64.ComputeIndex(input, 0), static data => FoldHashQuality64.ComputeHash(data)),
        new(nameof(FarmHash64), FarmHash64.ComputeIndex, static data => FarmHash64.ComputeHash(data)),
        new(nameof(Fnv1aHash64), Fnv1aHash64.ComputeIndex, Fnv1aHash64.ComputeHash),
        new(nameof(Gx2Hash64), static input => Gx2Hash64.ComputeIndex(input), static data => Gx2Hash64.ComputeHash(data)),
        new(nameof(HighwayHash64), HighwayHash64.ComputeIndex, HighwayHash64.ComputeHash),
        new(nameof(MarvinHash64), static input => MarvinHash64.ComputeIndex(input), static data => MarvinHash64.ComputeHash(data)),
        new(nameof(MeowHash64), MeowHash64.ComputeIndex, MeowHash64.ComputeHash),
        new(nameof(Polymur2Hash64), Polymur2Hash64.ComputeIndex, static data => Polymur2Hash64.ComputeHash(data)),
        new(nameof(Rapid3Hash64), static input => Rapid3Hash64.ComputeIndex(input, 0), static data => Rapid3Hash64.ComputeHash(data)),
        new(nameof(Rapid3HashMicro64), static input => Rapid3HashMicro64.ComputeIndex(input, 0), static data => Rapid3HashMicro64.ComputeHash(data)),
        new(nameof(Rapid3HashNano64), static input => Rapid3HashNano64.ComputeIndex(input, 0), static data => Rapid3HashNano64.ComputeHash(data)),
        new(nameof(SipHash64), static input => SipHash64.ComputeIndex(input), static data => SipHash64.ComputeHash(data)),
        new(nameof(Wy3Hash64), Wy3Hash64.ComputeIndex, static data => Wy3Hash64.ComputeHash(data)),
        new(nameof(Wy4Hash64), static input => Wy4Hash64.ComputeIndex(input), static data => Wy4Hash64.ComputeHash(data)),
        new(nameof(XxHash64), XxHash64.ComputeIndex, static data => XxHash64.ComputeHash(data)),
        new(nameof(Xx3Hash64), static input => Xx3Hash64.ComputeIndex(input), static data => Xx3Hash64.ComputeHash(data))
    ];

    public static readonly Index128Algorithm[] Index128Algorithms =
    [
        new(nameof(AesniHash128), static input => AesniHash128.ComputeIndex(input), static data => AesniHash128.ComputeHash(data)),
        new(nameof(CityHash128), static input => CityHash128.ComputeIndex(input), CityHash128.ComputeHash),
        new(nameof(FarmHash128), static input => FarmHash128.ComputeIndex(input), FarmHash128.ComputeHash),
        new(nameof(Gx2Hash128), static input => Gx2Hash128.ComputeIndex(input), static data => Gx2Hash128.ComputeHash(data)),
        new(nameof(MeowHash128), MeowHash128.ComputeIndex, MeowHash128.ComputeHash),
        new(nameof(Murmur3Hash128), static input => Murmur3Hash128.ComputeIndex(input), static data => Murmur3Hash128.ComputeHash(data)),
        new(nameof(Xx3Hash128), static input => Xx3Hash128.ComputeIndex(input), static data => Xx3Hash128.ComputeHash(data))
    ];

    public static Hash32Algorithm GetHash32(string name) => Hash32Algorithms.Single(x => x.Name == name);
    public static Hash64Algorithm GetHash64(string name) => Hash64Algorithms.Single(x => x.Name == name);
    public static Hash128Algorithm GetHash128(string name) => Hash128Algorithms.Single(x => x.Name == name);
    public static Index32Algorithm GetIndex32(string name) => Index32Algorithms.Single(x => x.Name == name);
    public static Index64Algorithm GetIndex64(string name) => Index64Algorithms.Single(x => x.Name == name);
    public static Index128Algorithm GetIndex128(string name) => Index128Algorithms.Single(x => x.Name == name);
}

public unsafe delegate uint Hash32Unsafe(byte* data, int length);
public unsafe delegate ulong Hash64Unsafe(byte* data, int length);
public unsafe delegate UInt128 Hash128Unsafe(byte* data, int length);
public delegate void Hash256(ReadOnlySpan<byte> data, Span<ulong> result);
public unsafe delegate void Hash256Unsafe(byte* data, int length, ulong* result);

public readonly record struct Hash32Algorithm(string Name, Func<ReadOnlySpan<byte>, uint>? Hash, Hash32Unsafe? UnsafeHash, byte[]? Expected)
{
    public override string ToString() => Name;
}

public readonly record struct Hash64Algorithm(string Name, Func<ReadOnlySpan<byte>, ulong>? Hash, Hash64Unsafe? UnsafeHash, byte[]? Expected)
{
    public override string ToString() => Name;
}

public readonly record struct Hash128Algorithm(string Name, Func<ReadOnlySpan<byte>, UInt128>? Hash, Hash128Unsafe? UnsafeHash, byte[]? Expected)
{
    public override string ToString() => Name;
}

public readonly record struct Hash256Algorithm(string Name, Hash256? Hash, Hash256Unsafe? UnsafeHash)
{
    public override string ToString() => Name;
}

public readonly record struct Index32Algorithm(string Name, Func<uint, uint> Index, Func<ReadOnlySpan<byte>, uint> Hash)
{
    public override string ToString() => Name;
}

public readonly record struct Index64Algorithm(string Name, Func<ulong, ulong> Index, Func<ReadOnlySpan<byte>, ulong> Hash)
{
    public override string ToString() => Name;
}

public readonly record struct Index128Algorithm(string Name, Func<ulong, UInt128> Index, Func<ReadOnlySpan<byte>, UInt128> Hash)
{
    public override string ToString() => Name;
}