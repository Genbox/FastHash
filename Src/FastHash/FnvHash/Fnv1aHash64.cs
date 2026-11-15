// C# implementation of FNV-1a by Glenn Fowler, Landon Curt Noll and Kiem-Phong Vo, which is in the public domain.
// See THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Provides the 64-bit FNV-1a hash algorithm.</summary>
public static class Fnv1aHash64
{
    /// <summary>Computes the hash of a 64-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 64-bit FNV-1a hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong hash = FNV1_64_INIT;
        hash = (hash ^ (input & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 8) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 16) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 24) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 32) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 40) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 48) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 56) & 0xFF)) * FNV_64_PRIME;
        return hash;
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit FNV-1a hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        int remaining = data.Length;
        if (remaining < 4)
            return Hash0To3(ref dataReference, remaining);

        ulong hash = FNV1_64_INIT;
        do
        {
            hash = (hash ^ dataReference) * FNV_64_PRIME;
            hash = (hash ^ Unsafe.Add(ref dataReference, 1)) * FNV_64_PRIME;
            hash = (hash ^ Unsafe.Add(ref dataReference, 2)) * FNV_64_PRIME;
            hash = (hash ^ Unsafe.Add(ref dataReference, 3)) * FNV_64_PRIME;
            dataReference = ref Unsafe.Add(ref dataReference, 4);
            remaining -= 4;
        } while (remaining >= 4);

        if (remaining >= 2)
        {
            hash = (hash ^ dataReference) * FNV_64_PRIME;
            hash = (hash ^ Unsafe.Add(ref dataReference, 1)) * FNV_64_PRIME;
            dataReference = ref Unsafe.Add(ref dataReference, 2);
            remaining -= 2;
        }

        if (remaining != 0)
            hash = (hash ^ dataReference) * FNV_64_PRIME;

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash0To3(ref byte data, int length)
    {
        if (length == 0)
            return FNV1_64_INIT;

        ulong hash = (FNV1_64_INIT ^ data) * FNV_64_PRIME;
        if (length > 1)
        {
            hash = (hash ^ Unsafe.Add(ref data, 1)) * FNV_64_PRIME;
            if (length > 2)
                hash = (hash ^ Unsafe.Add(ref data, 2)) * FNV_64_PRIME;
        }

        return hash;
    }
}