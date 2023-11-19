#if NET8_0
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.AesniHash;

public static class AesniHash128
{
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, uint seed = 0)
    {
        Vector128<byte> res = Hash128(data, seed);
        return Unsafe.As<Vector128<byte>, UInt128>(ref res);
    }

    [SuppressMessage("Major Code Smell", "S907:\"goto\" statement should not be used")]
    internal static Vector128<byte> Hash128(ReadOnlySpan<byte> data, uint seed = 0)
    {
        uint len = (uint)data.Length;

        Vector128<byte> a = Vector128.Create(seed).AsByte();
        Vector128<byte> b = Vector128.Create(len).AsByte();

        Vector128<byte> m = Vector128.Create(0x89abcdef, 0x01234567, 0xffff0000, 0xdeadbeef).AsByte();
        Vector128<byte> s = Vector128.Create((byte)12, 8, 4, 0, 13, 9, 5, 1, 14, 10, 6, 2, 15, 11, 7, 3);

        int msg = 0;

        if (len > 80)
        {
            Vector128<byte> c = Aes.Encrypt(b, m);
            Vector128<byte> d = Aes.Decrypt(a, m);
            a = Aes.Encrypt(a, m);
            b = Aes.Decrypt(b, m);

            do
            {
                a = Sse2.Xor(a, Vector128.Create(data[msg..]));
                b = Sse2.Xor(b, Vector128.Create(data[(msg + 16)..]));
                c = Sse2.Xor(c, Vector128.Create(data[(msg + 32)..]));
                d = Sse2.Xor(d, Vector128.Create(data[(msg + 48)..]));
                a = Ssse3.Shuffle(Aes.Encrypt(a, m), s);
                b = Ssse3.Shuffle(Aes.Decrypt(b, m), s);
                c = Ssse3.Shuffle(Aes.Encrypt(c, m), s);
                d = Ssse3.Shuffle(Aes.Decrypt(d, m), s);
                msg += 64;
                len -= 64;
            } while (len > 80);

            c = Aes.Encrypt(a, c);
            d = Aes.Decrypt(b, d);
            a = Aes.Encrypt(c, d);
            b = Aes.Decrypt(d, c);
        }

        while (len >= 16)
        {
            Mix(Vector128.Create(data[msg..]));
            msg += 16;
            len -= 16;
        }

        ulong x = 0;
        switch (len)
        {
            case 15:
                x |= (ulong)data[msg + 14] << 48;
                goto case 14;
            case 14:
                x |= (ulong)data[msg + 13] << 40;
                goto case 13;
            case 13:
                x |= (ulong)data[msg + 12] << 32;
                goto case 12;
            case 12:
                x |= BinaryPrimitives.ReadUInt32LittleEndian(data[(msg + 8)..]);
                Mix(Vector128.Create(BinaryPrimitives.ReadUInt64LittleEndian(data[msg..]), x).AsByte());
                break;
            case 11:
                x |= (uint)data[msg + 10] << 16;
                goto case 10;
            case 10:
                x |= (uint)data[msg + 9] << 8;
                goto case 9;
            case 9:
                x |= data[msg + 8];
                goto case 8;
            case 8:
                Mix(Vector128.Create(BinaryPrimitives.ReadUInt64LittleEndian(data), x).AsByte());
                break;
            case 7:
                x |= (ulong)data[msg + 4] << 48;
                goto case 6;
            case 6:
                x |= (ulong)data[msg + 4] << 40;
                goto case 5;
            case 5:
                x |= (ulong)data[msg + 4] << 32;
                goto case 4;
            case 4:
                x |= BinaryPrimitives.ReadUInt32LittleEndian(data[msg..]);
                Mix(Vector128.Create(x, 0).AsByte());
                break;
            case 3:
                x |= (uint)data[msg + 2] << 16;
                goto case 2;
            case 2:
                x |= (uint)data[msg + 1] << 8;
                goto case 1;
            case 1:
                x |= data[msg];
                Mix(Vector128.Create(x, 0).AsByte());
                break;
            default: // try to keep m & s from register spilling
                a = Sse2.Add(a, s);
                b = Sse2.Add(b, m);
                break;
        }

        return Aes.Encrypt(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Mix(Vector128<byte> x2)
        {
            a = Aes.Encrypt(x2, a);
            a = Aes.Encrypt(a, m);
            b = Ssse3.Shuffle(Sse2.Xor(x2, b), s);
            b = Ssse3.Shuffle(Aes.Decrypt(b, m), s);
        }
    }
}
#endif