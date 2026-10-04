#if NET10_0_OR_GREATER

// C# port of orlp/polyxor 0.1.0 (3123eb6). Copyright (c) 2026 Orson Peters.
// Distributed under the zlib license; see LICENSE.txt in this directory.
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.PolyXorHash;

/// <summary>A reusable streaming PolyXOR128 state. Instances are not thread-safe.</summary>
public sealed class PolyXorHasher
{
    private readonly PolyXorHashParams _parameters;
    private readonly byte[] _sponge = new byte[4096];
    private Vector128<ulong> _accum;

    /// <summary>Creates an empty hash state.</summary>
    /// <param name="parameters">Immutable parameters shared by hash states.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    public PolyXorHasher(PolyXorHashParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        PolyXorHashShared.RequireSupported();
        _parameters = parameters;
        _accum = parameters.PolyZ;
    }

    /// <summary>Gets the number of bytes hashed so far.</summary>
    public ulong Count { get; private set; }

    /// <summary>Adds input bytes to the state without allocating.</summary>
    /// <param name="data">The next bytes of the message.</param>
    public void Update(ReadOnlySpan<byte> data)
    {
        int buffered = (int)(Count & 4095);

        if (buffered != 0)
        {
            int absorb = Math.Min(data.Length, 4096 - buffered);
            data[..absorb].CopyTo(_sponge.AsSpan(buffered));
            Count += (ulong)absorb;
            if (buffered + absorb < 4096)
                return;
            PolyXorHashShared.HashBlocks(_sponge, _parameters, ref _accum);
            data = data[absorb..];
        }

        int whole = data.Length & ~4095;

        if (whole != 0)
        {
            PolyXorHashShared.HashBlocks(data[..whole], _parameters, ref _accum);
            Count += (ulong)whole;
            data = data[whole..];
        }

        data.CopyTo(_sponge);
        Count += (ulong)data.Length;
    }

    /// <summary>Returns the raw hash without consuming the state.</summary>
    /// <returns>The 128-bit universal hash.</returns>
    public UInt128 FinalizeRaw() => PolyXorHashShared.ToUInt128(Raw());

    /// <summary>Returns an avalanched hash without consuming the state.</summary>
    /// <param name="tweak">An optional 64-bit avalanche tweak.</param>
    /// <returns>The avalanched 128-bit hash.</returns>
    public UInt128 FinalizeAvalanche(ulong tweak = 0) =>
        PolyXorHashShared.ToUInt128(PolyXorHashShared.Avalanche(Raw(), _parameters, tweak));

    /// <summary>Returns a MAC without consuming the state.</summary>
    /// <param name="nonce">The nonce; the top two bits are ignored.</param>
    /// <returns>The 128-bit authentication tag.</returns>
    /// <exception cref="InvalidOperationException">The parameters were initialized from entropy instead of an AES key.</exception>
    /// <exception cref="PlatformNotSupportedException">Required AES intrinsics are unavailable.</exception>
    public UInt128 FinalizeMac(UInt128 nonce) => _parameters.Mac(Raw(), nonce);

    /// <summary>Resets the state for another message with the same parameters.</summary>
    public void Reset()
    {
        Count = 0;
        _accum = _parameters.PolyZ;
    }

    /// <summary>Copies this in-progress state, sharing only immutable parameters.</summary>
    /// <returns>An independently mutable state.</returns>
    public PolyXorHasher Clone()
    {
        PolyXorHasher clone = new PolyXorHasher(_parameters) { Count = Count, _accum = _accum };
        _sponge.AsSpan(0, (int)(Count & 4095)).CopyTo(clone._sponge);
        return clone;
    }

    private Vector128<ulong> Raw()
    {
        Vector128<ulong> accum = _accum;
        int buffered = (int)(Count & 4095);

        if (buffered != 0)
        {
            if (buffered <= sizeof(ulong))
            {
                // Use the same output-equivalent shortcut as one-shot hashing,
                // including short tails after full chunks. Upstream pads a real block.
                accum = PolyXorHashShared.AbsorbShort(PolyXorHashShared.ReadShort(_sponge.AsSpan(0, buffered)), accum, _parameters);
            }
            else
            {
                int padded = (buffered + 127) & ~127;
                _sponge.AsSpan(buffered, padded - buffered).Clear();
                PolyXorHashShared.HashBlocks(_sponge.AsSpan(0, padded), _parameters, ref accum);
            }
        }

        return PolyXorHashShared.Multiply(accum ^ _parameters.PolyU, Vector128.Create(Count, 0UL) ^ _parameters.PolyY);
    }
}
#endif