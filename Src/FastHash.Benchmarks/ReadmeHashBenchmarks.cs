using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ReadmeHashBenchmarks
{
    private readonly ulong[] _crcResult = new ulong[4];
    private readonly byte[] _md5 = new byte[16];
    private readonly byte[] _sha1 = new byte[20];
    private readonly byte[] _sha256 = new byte[32];
    private byte[] _data = null!;

    [Params(32 * 1024)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup() => _data = BenchmarkHelper.GetRandomBytes(Size);

    [Benchmark]
    public uint CityHash32() => CityHash.CityHash32.ComputeHash(_data);

    [Benchmark]
    public uint Djb2AltHash32() => DjbHash.Djb2AltHash32.ComputeHash(_data);

    [Benchmark]
    public uint Djb2Hash32() => DjbHash.Djb2Hash32.ComputeHash(_data);

    [Benchmark]
    public uint FarmHash32() => FarmHash.FarmHash32.ComputeHash(_data);

    [Benchmark]
    public uint FarshHash32() => FarshHash.FarshHash32.ComputeHash(_data);

    [Benchmark]
    public uint Fnv1aHash32() => FnvHash.Fnv1aHash32.ComputeHash(_data);

    [Benchmark]
    public uint Gx2Hash32() => GxHash.Gx2Hash32.ComputeHash(_data);

    [Benchmark]
    public uint MarvinHash32() => MarvinHash.MarvinHash32.ComputeHash(_data);

    [Benchmark]
    public uint Murmur3Hash32() => MurmurHash.Murmur3Hash32.ComputeHash(_data);

    [Benchmark]
    public uint SuperFastHash32() => SuperFastHash.SuperFastHash32.ComputeHash(_data);

    [Benchmark]
    public uint XxHash32() => XxHash.XxHash32.ComputeHash(_data);

    [Benchmark]
    public ulong AbslHash64() => AbslHash.AbslHash64.ComputeHash(_data);

    [Benchmark]
    public ulong AesniHash64() => AesniHash.AesniHash64.ComputeHash(_data);

    [Benchmark]
    public ulong CityHash64() => CityHash.CityHash64.ComputeHash(_data);

    [Benchmark]
    public ulong ClHash64() => ClHash.ClHash64.ComputeHash(_data);

    [Benchmark]
    public ulong Djb2AltHash64() => DjbHash.Djb2AltHash64.ComputeHash(_data);

    [Benchmark]
    public ulong Djb2Hash64() => DjbHash.Djb2Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong FarmHash64() => FarmHash.FarmHash64.ComputeHash(_data);

    [Benchmark]
    public ulong FarshHash64() => FarshHash.FarshHash64.ComputeHash(_data);

    [Benchmark]
    public ulong Fnv1aHash64() => FnvHash.Fnv1aHash64.ComputeHash(_data);

    [Benchmark]
    public ulong FoldHash64() => FoldHash.FoldHash64.ComputeHash(_data);

    [Benchmark]
    public ulong FoldHashQuality64() => FoldHash.FoldHashQuality64.ComputeHash(_data);

    [Benchmark]
    public ulong Gx2Hash64() => GxHash.Gx2Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong HighwayHash64() => HighwayHash.HighwayHash64.ComputeHash(_data);

    [Benchmark]
    public ulong MarvinHash64() => MarvinHash.MarvinHash64.ComputeHash(_data);

    [Benchmark]
    public ulong MeowHash64() => MeowHash.MeowHash64.ComputeHash(_data);

    [Benchmark]
    public ulong Polymur2Hash64() => PolymurHash.Polymur2Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong Rapid3Hash64() => RapidHash.Rapid3Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong Rapid3HashMicro64() => RapidHash.Rapid3HashMicro64.ComputeHash(_data);

    [Benchmark]
    public ulong Rapid3HashNano64() => RapidHash.Rapid3HashNano64.ComputeHash(_data);

    [Benchmark]
    public ulong SipHash64() => SipHash.SipHash64.ComputeHash(_data);

    [Benchmark]
    public ulong T1ha2Hash64() => T1haHash.T1ha2Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong Wy2Hash64() => WyHash.Wy2Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong Wy3Hash64() => WyHash.Wy3Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong Wy4Hash64() => WyHash.Wy4Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong Xx3Hash64() => XxHash.Xx3Hash64.ComputeHash(_data);

    [Benchmark]
    public ulong XxHash64() => XxHash.XxHash64.ComputeHash(_data);

    [Benchmark]
    public UInt128 AesniHash128() => AesniHash.AesniHash128.ComputeHash(_data);

    [Benchmark]
    public UInt128 CityHash128() => CityHash.CityHash128.ComputeHash(_data);

    [Benchmark]
    public UInt128 CityHashCrc128() => CityHash.CityHashCrc128.ComputeHash(_data);

    [Benchmark]
    public UInt128 FarmHash128() => FarmHash.FarmHash128.ComputeHash(_data);

    [Benchmark]
    public UInt128 Gx2Hash128() => GxHash.Gx2Hash128.ComputeHash(_data);

    [Benchmark]
    public UInt128 MeowHash128() => MeowHash.MeowHash128.ComputeHash(_data);

    [Benchmark]
    public UInt128 Murmur3Hash128() => MurmurHash.Murmur3Hash128.ComputeHash(_data);

    [Benchmark]
    public UInt128 PolyXorHash128() => PolyXorHash.PolyXorHash128.ComputeHash(_data);

    [Benchmark]
    public UInt128 Xx3Hash128() => XxHash.Xx3Hash128.ComputeHash(_data);

    [Benchmark]
    public void CityHashCrc256() => CityHash.CityHashCrc256.ComputeHash(_data, _crcResult);

    [Benchmark]
    public void MD5() => System.Security.Cryptography.MD5.HashData(_data, _md5);

    [Benchmark]
    public void SHA1() => System.Security.Cryptography.SHA1.HashData(_data, _sha1);

    [Benchmark]
    public void SHA256() => System.Security.Cryptography.SHA256.HashData(_data, _sha256);
}