using System.Runtime.InteropServices;

namespace Genbox.FastHash;

[StructLayout(LayoutKind.Sequential)]
public struct UInt128(ulong low, ulong high)
{
    public ulong Low = low;
    public ulong High = high;

    public override string ToString()
    {
        return Low + "," + High;
    }
}