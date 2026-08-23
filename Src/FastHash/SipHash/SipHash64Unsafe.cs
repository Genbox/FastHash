using System.Runtime.CompilerServices;
using static Genbox.FastHash.SipHash.SipHashConstants;

namespace Genbox.FastHash.SipHash;

/// <summary>Provides unsafe 64-bit SipHash computations over unmanaged memory.</summary>
public static class SipHash64Unsafe
{
    /// <summary>Computes a 64-bit hash for an unmanaged byte sequence using default seeds and rounds.</summary>
    /// <param name="data">A pointer to at least <paramref name="length"/> bytes, or null when <paramref name="length"/> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, 0, 0);
    }

    /// <summary>Computes a 64-bit hash for an unmanaged byte sequence using <paramref name="seed1"/> and default rounds.</summary>
    /// <param name="data">A pointer to at least <paramref name="length"/> bytes, or null when <paramref name="length"/> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed1">The first hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, seed1, 0);
    }

    /// <summary>Computes a 64-bit hash for an unmanaged byte sequence using the supplied seeds and default rounds.</summary>
    /// <param name="data">A pointer to at least <paramref name="length"/> bytes, or null when <paramref name="length"/> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed1">The first hash seed.</param>
    /// <param name="seed2">The second hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1, ulong seed2)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, seed1, seed2, 2, 4);
    }

    /// <summary>Computes a 64-bit hash for an unmanaged byte sequence using the supplied seeds and rounds.</summary>
    /// <param name="data">A pointer to at least <paramref name="length"/> bytes, or null when <paramref name="length"/> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed1">The first hash seed.</param>
    /// <param name="seed2">The second hash seed.</param>
    /// <param name="cRounds">The number of compression rounds.</param>
    /// <param name="dRounds">The number of finalization rounds.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1, ulong seed2, byte cRounds, byte dRounds)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        ulong v0 = v0Init;
        ulong v1 = v1Init;
        ulong v2 = v2Init;
        ulong v3 = v3Init;

        int left = length & 7;
        ulong b = (ulong)length << 56;
        int num = length / 8;
        int offset1 = length - left;
        int i;

        v3 ^= seed2;
        v2 ^= seed1;
        v1 ^= seed2;
        v0 ^= seed1;

        for (int block = 0; block < num; block++)
        {
            ulong m = Read64(data + (block * sizeof(ulong)));
            v3 ^= m;

            for (i = 0; i < cRounds; ++i)
                SipRound(ref v0, ref v1, ref v2, ref v3);

            v0 ^= m;
        }

        switch (left)
        {
            case 7:
                b |= (ulong)data[6 + offset1] << 48;
                goto case 6;
            case 6:
                b |= (ulong)data[5 + offset1] << 40;
                goto case 5;
            case 5:
                b |= (ulong)data[4 + offset1] << 32;
                goto case 4;
            case 4:
                b |= (ulong)data[3 + offset1] << 24;
                goto case 3;
            case 3:
                b |= (ulong)data[2 + offset1] << 16;
                goto case 2;
            case 2:
                b |= (ulong)data[1 + offset1] << 8;
                goto case 1;
            case 1:
                b |= data[0 + offset1];
                break;
            case 0:
                break;
        }

        v3 ^= b;

        for (i = 0; i < cRounds; ++i)
            SipRound(ref v0, ref v1, ref v2, ref v3);

        v0 ^= b;
        v2 ^= 0xFF;

        for (i = 0; i < dRounds; ++i)
            SipRound(ref v0, ref v1, ref v2, ref v3);

        return v0 ^ v1 ^ v2 ^ v3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SipRound(ref ulong v0, ref ulong v1, ref ulong v2, ref ulong v3)
    {
        v0 += v1;
        v1 = RotateLeft(v1, 13);
        v1 ^= v0;
        v0 = RotateLeft(v0, 32);

        v2 += v3;
        v3 = RotateLeft(v3, 16);
        v3 ^= v2;

        v2 += v1;
        v1 = RotateLeft(v1, 17);
        v1 ^= v2;
        v2 = RotateLeft(v2, 32);

        v0 += v3;
        v3 = RotateLeft(v3, 21);
        v3 ^= v0;
    }
}