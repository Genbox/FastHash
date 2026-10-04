// C# port of google/highwayhash (f8381f3). Copyright 2017 Google Inc. All Rights Reserved.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
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