using System.Runtime.InteropServices;

namespace Genbox.FastHash;

[StructLayout(LayoutKind.Sequential)]
public struct UInt128
{
    public static UInt128 Zero;

    public UInt128(ulong low, ulong high)
    {
        Low = low;
        High = high;
    }

    public ulong Low;
    public ulong High;
}