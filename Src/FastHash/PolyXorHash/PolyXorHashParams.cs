#if NET10_0_OR_GREATER

// C# port of orlp/polyxor 0.1.0 (3123eb6). Copyright (c) 2026 Orson Peters.
// Distributed under the zlib license; see LICENSE.txt in this directory.
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using X86Aes = System.Runtime.Intrinsics.X86.Aes;
using ArmAes = System.Runtime.Intrinsics.Arm.Aes;
using Aes = System.Security.Cryptography.Aes;

namespace Genbox.FastHash.PolyXorHash;

/// <summary>Contains reusable, immutable PolyXOR128 parameters derived from a key or entropy.</summary>
public sealed class PolyXorHashParams
{
    /// <summary>The number of uniformly random bytes needed to initialize the parameters.</summary>
    public const int EntropyNeeded = 4160;

    internal readonly byte[] BlockKey;
    internal readonly Vector128<ulong> PolyZ;
    internal readonly Vector128<ulong> PolyU;
    internal readonly Vector128<ulong> PolyY;
    internal readonly Vector128<ulong> AvalancheMul;
    internal readonly Vector128<ulong> IndexH0;
    internal readonly Vector128<ulong> IndexH1;
    internal readonly Vector128<ulong> IndexMultipliers;
    private readonly bool _hasAesKey;
    private readonly Vector128<byte>[]? _roundKeys;

    /// <summary>Expands a 128-bit key using upstream's AES-128 counter construction.</summary>
    /// <param name="key">The key, encoded as little-endian low and high words.</param>
    /// <exception cref="CryptographicException">AES key expansion failed.</exception>
    public PolyXorHashParams(UInt128 key)
    {
        _hasAesKey = true;
        Span<byte> entropy = stackalloc byte[EntropyNeeded];
        Span<byte> keyBytes = stackalloc byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(keyBytes, key.Low);
        BinaryPrimitives.WriteUInt64LittleEndian(keyBytes[8..], key.High);
        _roundKeys = X86Aes.IsSupported || ArmAes.IsSupported ? ExpandKey(keyBytes) : null;
        for (int i = 0; i < EntropyNeeded / 16; i++)
            Vector128.Create((ulong)i, 1UL << 63).AsByte().StoreUnsafe(ref entropy[i * 16]);

        if (X86Aes.IsSupported || ArmAes.IsSupported)
        {
            for (int i = 0; i < entropy.Length; i += 32)
            {
                Vector128<byte> a = Vector128.LoadUnsafe(ref entropy[i]);
                Vector128<byte> b = Vector128.LoadUnsafe(ref entropy[i + 16]);
                EncryptPair(ref a, ref b);
                a.StoreUnsafe(ref entropy[i]);
                b.StoreUnsafe(ref entropy[i + 16]);
            }
        }
        else
        {
            // Parameter construction is also used by reference tests with intrinsics disabled.
            using Aes aes = Aes.Create();
            aes.Key = keyBytes.ToArray();
            aes.EncryptEcb(entropy, entropy, PaddingMode.None);
        }

        PolyZ = Load(entropy);
        PolyU = Load(entropy[16..]);
        PolyY = Load(entropy[32..]);
        AvalancheMul = Nonzero(Load(entropy[48..]));
        BlockKey = entropy[64..].ToArray();
        PolyXorHashShared.PrepareIndex(this, out IndexH0, out IndexH1, out IndexMultipliers);
    }

    private PolyXorHashParams(ReadOnlySpan<byte> entropy)
    {
        if (entropy.Length < EntropyNeeded)
            throw new ArgumentException($"At least {EntropyNeeded} bytes of entropy are required.", nameof(entropy));

        PolyZ = Load(entropy);
        PolyU = Load(entropy[16..]);
        PolyY = Load(entropy[32..]);
        AvalancheMul = Nonzero(Load(entropy[48..]));
        BlockKey = entropy.Slice(64, 4096).ToArray();
        PolyXorHashShared.PrepareIndex(this, out IndexH0, out IndexH1, out IndexMultipliers);
    }

    /// <summary>Creates parameters from uniformly random bytes. These parameters cannot produce MACs.</summary>
    /// <param name="entropy">At least <see cref="EntropyNeeded"/> bytes; extra bytes are ignored.</param>
    /// <returns>The expanded parameters.</returns>
    /// <exception cref="ArgumentException"><paramref name="entropy" /> contains fewer than <see cref="EntropyNeeded" /> bytes.</exception>
    public static PolyXorHashParams FromEntropy(ReadOnlySpan<byte> entropy) => new PolyXorHashParams(entropy);

    /// <summary>Starts a new streaming hash using these parameters.</summary>
    /// <returns>A reusable streaming hasher.</returns>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    public PolyXorHasher CreateHasher() => new PolyXorHasher(this);

    internal UInt128 Mac(Vector128<ulong> raw, UInt128 nonce)
    {
        if (!_hasAesKey)
            throw new InvalidOperationException("MAC output requires parameters initialized with an AES key.");
        if (!X86Aes.IsSupported && !ArmAes.IsSupported)
            throw new PlatformNotSupportedException("Hardware AES is required for PolyXOR MAC output.");

        Vector128<ulong> n = Vector128.Create(nonce.Low, nonce.High);
        Vector128<ulong> mask = Vector128.Create(ulong.MaxValue, ulong.MaxValue >> 2);
        Vector128<byte> a = (n & mask).AsByte();
        Vector128<byte> b = (((raw ^ n) & mask) | Vector128.Create(0UL, 1UL << 62)).AsByte();
        EncryptPair(ref a, ref b);
        return PolyXorHashShared.ToUInt128((a ^ b).AsUInt64());
    }

    private void EncryptPair(ref Vector128<byte> a, ref Vector128<byte> b)
    {
        Vector128<byte>[] keys = _roundKeys!;

        if (X86Aes.IsSupported)
        {
            a ^= keys[0];
            b ^= keys[0];

            for (int i = 1; i < 10; i++)
            {
                a = X86Aes.Encrypt(a, keys[i]);
                b = X86Aes.Encrypt(b, keys[i]);
            }

            a = X86Aes.EncryptLast(a, keys[10]);
            b = X86Aes.EncryptLast(b, keys[10]);
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                a = ArmAes.MixColumns(ArmAes.Encrypt(a, keys[i]));
                b = ArmAes.MixColumns(ArmAes.Encrypt(b, keys[i]));
            }

            a = ArmAes.Encrypt(a, keys[9]) ^ keys[10];
            b = ArmAes.Encrypt(b, keys[9]) ^ keys[10];
        }
    }

    private static Vector128<ulong> Load(ReadOnlySpan<byte> bytes) => Vector128.Create(BinaryPrimitives.ReadUInt64LittleEndian(bytes), BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..]));

    private static Vector128<ulong> Nonzero(Vector128<ulong> value) =>
        value.GetElement(0) == 0 && value.GetElement(1) == 0 ? Vector128.Create(1UL, 0UL) : value;

    private static Vector128<byte>[] ExpandKey(ReadOnlySpan<byte> key)
    {
        Vector128<byte>[] keys = new Vector128<byte>[11];
        keys[0] = Vector128.Create(BinaryPrimitives.ReadUInt64LittleEndian(key), BinaryPrimitives.ReadUInt64LittleEndian(key[8..])).AsByte();

        if (X86Aes.IsSupported)
        {
            keys[1] = ExpandX86(keys[0], X86Aes.KeygenAssist(keys[0], 0x01));
            keys[2] = ExpandX86(keys[1], X86Aes.KeygenAssist(keys[1], 0x02));
            keys[3] = ExpandX86(keys[2], X86Aes.KeygenAssist(keys[2], 0x04));
            keys[4] = ExpandX86(keys[3], X86Aes.KeygenAssist(keys[3], 0x08));
            keys[5] = ExpandX86(keys[4], X86Aes.KeygenAssist(keys[4], 0x10));
            keys[6] = ExpandX86(keys[5], X86Aes.KeygenAssist(keys[5], 0x20));
            keys[7] = ExpandX86(keys[6], X86Aes.KeygenAssist(keys[6], 0x40));
            keys[8] = ExpandX86(keys[7], X86Aes.KeygenAssist(keys[7], 0x80));
            keys[9] = ExpandX86(keys[8], X86Aes.KeygenAssist(keys[8], 0x1b));
            keys[10] = ExpandX86(keys[9], X86Aes.KeygenAssist(keys[9], 0x36));
        }
        else
        {
            uint rc = 1;

            for (int i = 1; i < keys.Length; i++)
            {
                Vector128<uint> previous = keys[i - 1].AsUInt32();

                // Equal columns make AESE's ShiftRows a no-op, giving SubWord
                // without a secret-indexed S-box table.
                uint rotated = BitOperations.RotateRight(previous.GetElement(3), 8);
                uint sub = ArmAes.Encrypt(Vector128.Create(rotated).AsByte(), Vector128<byte>.Zero).AsUInt32().GetElement(0);
                uint a = previous.GetElement(0) ^ sub ^ rc;
                uint b = previous.GetElement(1) ^ a;
                uint c = previous.GetElement(2) ^ b;
                uint d = previous.GetElement(3) ^ c;
                keys[i] = Vector128.Create(a, b, c, d).AsByte();
                rc = ((rc << 1) ^ ((rc >> 7) * 0x1b)) & 255;
            }
        }

        return keys;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> ExpandX86(Vector128<byte> key, Vector128<byte> assist)
    {
        key ^= Sse2.ShiftLeftLogical128BitLane(key, 4);
        key ^= Sse2.ShiftLeftLogical128BitLane(key, 8);
        return key ^ Sse2.Shuffle(assist.AsUInt32(), 0xff).AsByte();
    }
}
#endif