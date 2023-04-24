using System.Runtime.InteropServices;

namespace Genbox.FastHash;

[StructLayout(LayoutKind.Sequential)]
public struct UInt96
{
    public static UInt96 Zero;

    public UInt96(uint first, uint second, uint third)
    {
        First = first;
        Second = second;
        Third = third;
    }

    public uint First;
    public uint Second;
    public uint Third;
}