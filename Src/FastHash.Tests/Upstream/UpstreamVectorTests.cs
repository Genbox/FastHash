using System.Globalization;
using System.Reflection;
using Genbox.FastHash.AbslHash;
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
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.T1haHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;
using ArmAes = System.Runtime.Intrinsics.Arm.Aes;
using X86Aes = System.Runtime.Intrinsics.X86.Aes;

namespace Genbox.FastHash.Tests.Upstream;

/// <summary>
/// Checks every hash against vectors produced by compiling its upstream source (see the comments in Upstream/Vectors/*.txt).
/// Each vector hashes the first Length bytes of <see cref="Data" />.
/// </summary>
public class UpstreamVectorTests
{
    private static readonly byte[] Data = CreateData();
    private static readonly Dictionary<string, List<Vector>> Vectors = LoadVectors();

    private static readonly Dictionary<string, Entry> Hashers = new Dictionary<string, Entry>(StringComparer.Ordinal)
    {
        ["AbslHash64Portable"] = H64(static (d, v) => AbslHash64.ComputeHash(d, v.Seed1), static () => !AbslHash64.IsSimdSupported),
        ["AbslHash64Aes"] = H64(static (d, v) => AbslHash64.ComputeHash(d, v.Seed1), static () => AbslHash64.IsSimdSupported),
        ["CityHash32"] = H32(static (d, _) => CityHash32.ComputeHash(d)),
        ["CityHash64"] = H64(static (d, _) => CityHash64.ComputeHash(d)),
        ["CityHash64WithSeed"] = H64(static (d, v) => CityHash64.ComputeHash(d, v.Seed1)),
        ["CityHash64WithSeeds"] = H64(static (d, v) => CityHash64.ComputeHash(d, v.Seed1, v.Seed2)),
        ["CityHash128"] = H128(static (d, _) => CityHash128.ComputeHash(d)),
        ["CityHash128WithSeed"] = H128(static (d, v) => CityHash128.ComputeHash(d, new UInt128(v.Seed2, v.Seed1))),
        ["CityHashCrc128"] = H128(static (d, _) => CityHashCrc128.ComputeHash(d), static () => CityHashCrc128.IsSupported),
        ["CityHashCrc128WithSeed"] = H128(static (d, v) => CityHashCrc128.ComputeHash(d, new UInt128(v.Seed2, v.Seed1)), static () => CityHashCrc128.IsSupported),
        ["CityHashCrc256"] = new Entry(static (d, _, r) => CityHashCrc256.ComputeHash(d, r), static () => CityHashCrc256.IsSupported),
        ["ClHash64"] = H64(static (d, v) => ClHash64.ComputeHash(d, v.Seed1, v.Seed2), static () => ClHash64.IsSupported),
        ["Djb2Hash32"] = H32(static (d, _) => Djb2Hash32.ComputeHash(d)),
        ["Djb2Hash64"] = H64(static (d, _) => Djb2Hash64.ComputeHash(d)),
        ["Djb2AltHash32"] = H32(static (d, _) => Djb2AltHash32.ComputeHash(d)),
        ["Djb2AltHash64"] = H64(static (d, _) => Djb2AltHash64.ComputeHash(d)),
        ["FarmHash32"] = H32(static (d, _) => FarmHash32.ComputeHash(d)),
        ["FarmHash32WithSeed"] = H32(static (d, v) => FarmHash32.ComputeHash(d, (uint)v.Seed1)),
        ["FarmHash64"] = H64(static (d, _) => FarmHash64.ComputeHash(d)),
        ["FarmHash64WithSeed"] = H64(static (d, v) => FarmHash64.ComputeHash(d, v.Seed1)),
        ["FarmHash64WithSeeds"] = H64(static (d, v) => FarmHash64.ComputeHash(d, v.Seed1, v.Seed2)),
        ["FarmHash128"] = H128(static (d, _) => FarmHash128.ComputeHash(d)),
        ["FarmHash128WithSeed"] = H128(static (d, v) => FarmHash128.ComputeHash(d, new UInt128(v.Seed2, v.Seed1))),
        ["FarshHash32"] = H32(static (d, v) => FarshHash32.ComputeHash(d, v.Seed1)),
        ["FarshHash64"] = H64(static (d, v) => FarshHash64.ComputeHash(d, v.Seed1)),
        ["Fnv1aHash32"] = H32(static (d, _) => Fnv1aHash32.ComputeHash(d)),
        ["Fnv1aHash64"] = H64(static (d, _) => Fnv1aHash64.ComputeHash(d)),
        ["FoldHash64"] = H64(static (d, v) => FoldHash64.ComputeHash(d, v.Seed1)),
        ["FoldHashQuality64"] = H64(static (d, v) => FoldHashQuality64.ComputeHash(d, v.Seed1)),
        ["Gx2Hash32"] = H32(static (d, v) => Gx2Hash32.ComputeHash(d, unchecked((long)v.Seed1)), HasAes),
        ["Gx2Hash64"] = H64(static (d, v) => Gx2Hash64.ComputeHash(d, unchecked((long)v.Seed1)), HasAes),
        ["Gx2Hash128"] = H128(static (d, v) => Gx2Hash128.ComputeHash(d, unchecked((long)v.Seed1)), HasAes),
        ["HighwayHash64"] = H64(static (d, v) => HighwayHash64.ComputeHash(d, v.Seed1, v.Seed2, v.Seed3, v.Seed4)),
        ["MarvinHash32"] = H32(static (d, v) => MarvinHash32.ComputeHash(d, v.Seed1)),
        ["MeowHash64"] = H64(static (d, _) => MeowHash64.ComputeHash(d), static () => MeowHash64.IsSupported),
        ["MeowHash128"] = H128(static (d, _) => MeowHash128.ComputeHash(d), static () => MeowHash128.IsSupported),
        ["Murmur3Hash32"] = H32(static (d, v) => Murmur3Hash32.ComputeHash(d, (uint)v.Seed1)),
        ["Murmur3Hash128"] = H128(static (d, v) => Murmur3Hash128.ComputeHash(d, (uint)v.Seed1)),
        ["Polymur2Hash64"] = H64(static (d, v) => Polymur2Hash64.ComputeHash(d, v.Seed1, v.Seed2)),
        ["PolyXorHash128"] = H128(static (d, v) => PolyXorHash128.ComputeHash(d, new UInt128(v.Seed2, v.Seed1), v.Seed3), static () => PolyXorHash128.IsSupported),
        ["SipHash64"] = H64(static (d, v) => SipHash64.ComputeHash(d, v.Seed1, v.Seed2)),
        ["SipHash13"] = H64(static (d, v) => SipHash64.ComputeHash(d, v.Seed1, v.Seed2, 1, 3)),
        ["SuperFastHash32"] = H32(static (d, _) => SuperFastHash32.ComputeHash(d)),
        ["SuperFastHash32WithSeed"] = H32(static (d, v) => SuperFastHash32.ComputeHash(d, (uint)v.Seed1)),
        ["T1ha2Hash64"] = H64(static (d, v) => T1ha2Hash64.ComputeHash(d, v.Seed1)),
        ["Wy2Hash64"] = H64(static (d, v) => Wy2Hash64.ComputeHash(d, v.Seed1)),
        ["Wy2Hash64Secret"] = H64(static (d, v) => Wy2Hash64.ComputeHash(d, v.Seed1, v.Secret)),
        ["Wy3Hash64"] = H64(static (d, v) => Wy3Hash64.ComputeHash(d, v.Seed1)),
        ["Wy3Hash64Secret"] = H64(static (d, v) => Wy3Hash64.ComputeHash(d, v.Seed1, v.Secret)),
        ["Wy4Hash64"] = H64(static (d, v) => Wy4Hash64.ComputeHash(d, v.Seed1)),
        ["Wy4Hash64Secret"] = H64(static (d, v) => Wy4Hash64.ComputeHash(d, v.Seed1, v.Secret)),
        ["XxHash32"] = H32(static (d, v) => XxHash32.ComputeHash(d, (uint)v.Seed1)),
        ["XxHash64"] = H64(static (d, v) => XxHash64.ComputeHash(d, v.Seed1)),
        ["Xx3Hash64"] = H64(static (d, v) => Xx3Hash64.ComputeHash(d, v.Seed1)),
        ["Xx3Hash128"] = H128(static (d, v) => Xx3Hash128.ComputeHash(d, v.Seed1))
    };

    private delegate void Hasher(ReadOnlySpan<byte> data, Vector vector, Span<ulong> result);

    public static TheoryData<string> Variants() => new TheoryData<string>(Vectors.Keys.Order(StringComparer.Ordinal));

    [Theory]
    [MemberData(nameof(Variants))]
    public void MatchesUpstream(string variant)
    {
        Assert.True(Hashers.TryGetValue(variant, out Entry entry), $"No FastHash function is mapped to '{variant}'.");

        if (!entry.IsSupported())
            return;

        ulong[] actual = new ulong[4];

        foreach (Vector vector in Vectors[variant])
        {
            Array.Clear(actual);
            entry.Hash(Data.AsSpan(0, vector.Length), vector, actual);

            if (!actual.AsSpan().SequenceEqual(vector.Expected))
                Assert.Fail($"{variant}, length {vector.Length}, seeds {Hex(vector.Seed1, vector.Seed2, vector.Seed3, vector.Seed4)}: expected {Hex(vector.Expected)}, got {Hex(actual)}");
        }
    }

    [Fact]
    public void EveryHasherHasVectors() => Assert.Empty(Hashers.Keys.Except(Vectors.Keys, StringComparer.Ordinal));

    private static Entry H32(Func<ReadOnlySpan<byte>, Vector, uint> hash, Func<bool>? isSupported = null) => new Entry((d, v, r) => r[0] = hash(d, v), isSupported);
    private static Entry H64(Func<ReadOnlySpan<byte>, Vector, ulong> hash, Func<bool>? isSupported = null) => new Entry((d, v, r) => r[0] = hash(d, v), isSupported);

    private static Entry H128(Func<ReadOnlySpan<byte>, Vector, UInt128> hash, Func<bool>? isSupported = null) => new Entry((d, v, r) =>
    {
        UInt128 h = hash(d, v);
        r[0] = h.Low;
        r[1] = h.High;
    }, isSupported);

    private static bool HasAes() => X86Aes.IsSupported || ArmAes.IsSupported;

    private static string Hex(params ulong[] words) => "[" + string.Join(", ", words.Select(static w => "0x" + w.ToString("x", CultureInfo.InvariantCulture))) + "]";

    private static byte[] CreateData()
    {
        byte[] data = new byte[4097];

        for (int i = 0; i < data.Length; i++)
            data[i] = unchecked((byte)((i * 7) + 3));

        return data;
    }

    // Line format: "<variant> <length> <seed1> <seed2> <seed3> <seed4> <word0> <word1> <word2> <word3>" in hex (length in decimal),
    // or "secret <variant> <w0> <w1> <w2> <w3>". Lines starting with '#' are comments.
    private static Dictionary<string, List<Vector>> LoadVectors()
    {
        Assembly assembly = typeof(UpstreamVectorTests).Assembly;
        Dictionary<string, List<Vector>> vectors = new Dictionary<string, List<Vector>>(StringComparer.Ordinal);
        Dictionary<string, ulong[]> secrets = new Dictionary<string, ulong[]>(StringComparer.Ordinal);

        foreach (string name in assembly.GetManifestResourceNames().Where(static n => n.Contains(".Upstream.Vectors.", StringComparison.Ordinal)))
        {
            using StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(name)!);

            while (reader.ReadLine() is { } line)
            {
                if (line.Length == 0 || line[0] == '#')
                    continue;

                string[] p = line.Split(' ');

                if (p[0] == "secret")
                {
                    secrets[p[1]] = p[2..].Select(ParseHex).ToArray();
                    continue;
                }

                if (!vectors.TryGetValue(p[0], out List<Vector>? list))
                    vectors[p[0]] = list = [];

                list.Add(new Vector(int.Parse(p[1], CultureInfo.InvariantCulture), ParseHex(p[2]), ParseHex(p[3]), ParseHex(p[4]), ParseHex(p[5]),
                    p[6..10].Select(ParseHex).ToArray(), secrets.GetValueOrDefault(p[0])));
            }
        }

        return vectors;
    }

    private static ulong ParseHex(string value) => ulong.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    private readonly record struct Entry(Hasher Hash, Func<bool>? Supported)
    {
        public bool IsSupported() => Supported?.Invoke() ?? true;
    }

    private sealed record Vector(int Length, ulong Seed1, ulong Seed2, ulong Seed3, ulong Seed4, ulong[] Expected, ulong[]? Secret);
}
