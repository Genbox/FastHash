using System.Runtime.InteropServices;

namespace Genbox.FastHash.HighwayHash;

[StructLayout(LayoutKind.Auto)]
internal struct HighwayHashState
{
    internal ulong mul0_0;
    internal ulong mul0_1;
    internal ulong mul0_2;
    internal ulong mul0_3;
    internal ulong mul1_0;
    internal ulong mul1_1;
    internal ulong mul1_2;
    internal ulong mul1_3;
    internal ulong v0_0;
    internal ulong v0_1;
    internal ulong v0_2;
    internal ulong v0_3;
    internal ulong v1_0;
    internal ulong v1_1;
    internal ulong v1_2;
    internal ulong v1_3;
}