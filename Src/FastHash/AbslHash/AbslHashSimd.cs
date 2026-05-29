#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.AbslHash.AbslHashConstants;

namespace Genbox.FastHash.AbslHash;

internal static class AbslHashSimd
{
    internal static bool IsSupported => Aes.IsSupported && Sse2.IsSupported && Sse42.IsSupported;

    internal static ulong CombineLargeContiguous(ulong state, ReadOnlySpan<byte> data)
    {
        int len = data.Length;

        if (len <= PiecewiseChunkSize)
            return LowLevelHashLenGt32(state, data, len);

        while (len >= PiecewiseChunkSize)
        {
            state = LowLevelHashLenGt32(state, data.Slice(0, PiecewiseChunkSize), PiecewiseChunkSize);
            data = data.Slice(PiecewiseChunkSize);
            len -= PiecewiseChunkSize;
        }

        return len == 0 ? state : AbslHashShared.CombineContiguous(state, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHashLenGt32(ulong seed, ReadOnlySpan<byte> data, int len) => len > 64 ? LowLevelHashLenGt64(seed, data, len) : LowLevelHash33To64(seed, data, len);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHash33To64(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        Vector128<byte> state = Set128(seed, (ulong)len);
        Vector128<byte> a = Load128(data, 0);
        Vector128<byte> b = Load128(data, 16);
        int last32 = len - 32;
        Vector128<byte> c = Load128(data, last32);
        Vector128<byte> d = Load128(data, last32 + 16);

        Vector128<byte> na = MixA(a, state);
        Vector128<byte> nb = MixB(b, state);
        Vector128<byte> nc = MixC(c, state);
        Vector128<byte> nd = MixD(d, state);

        return Mix4x16Vectors(na, nb, nc, nd);
    }

    private static ulong LowLevelHashLenGt64(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        int last32 = len - 32;
        int offset = 0;

        Vector128<byte> state0 = Set128(seed, (ulong)len);
        Vector128<byte> state1 = state0;
        Vector128<byte> state2 = state1;
        Vector128<byte> state3 = state2;

        do
        {
            MixAb(data, offset, ref state0, ref state1);
            MixCd(data, offset + 32, ref state2, ref state3);

            offset += 64;
            len -= 64;
        } while (len > 64);

        if (len > 32)
            MixAb(data, offset, ref state0, ref state1);

        MixCd(data, last32, ref state2, ref state3);

        return Mix4x16Vectors(state0, state1, state2, state3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MixAb(ReadOnlySpan<byte> data, int offset, ref Vector128<byte> state0, ref Vector128<byte> state1)
    {
        Vector128<byte> a = Load128(data, offset);
        Vector128<byte> b = Load128(data, offset + 16);
        state0 = MixA(a, state0);
        state1 = MixB(b, state1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MixCd(ReadOnlySpan<byte> data, int offset, ref Vector128<byte> state2, ref Vector128<byte> state3)
    {
        Vector128<byte> c = Load128(data, offset);
        Vector128<byte> d = Load128(data, offset + 16);
        state2 = MixC(c, state2);
        state3 = MixD(d, state3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Load128(ReadOnlySpan<byte> data, int offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Set128(ulong a, ulong b) => Vector128.Create(b, a).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Add128(Vector128<byte> a, Vector128<byte> b) => Sse2.Add(a.AsUInt64(), b.AsUInt64()).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Sub128(Vector128<byte> a, Vector128<byte> b) => Sse2.Subtract(a.AsUInt64(), b.AsUInt64()).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixA(Vector128<byte> a, Vector128<byte> state) => Aes.Decrypt(Add128(state, a), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixB(Vector128<byte> b, Vector128<byte> state) => Aes.Decrypt(Sub128(state, b), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixC(Vector128<byte> c, Vector128<byte> state) => Aes.Encrypt(Add128(state, c), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixD(Vector128<byte> d, Vector128<byte> state) => Aes.Encrypt(Sub128(state, d), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix4x16Vectors(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c, Vector128<byte> d)
    {
        Vector128<byte> res = Add128(Aes.Encrypt(Add128(a, c), d), Aes.Decrypt(Sub128(b, d), a));
        Vector128<ulong> res64 = res.AsUInt64();
        return res64.GetElement(0) ^ res64.GetElement(1);
    }
}
#endif