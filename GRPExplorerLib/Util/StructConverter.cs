using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GRPExplorerLib.Util
{
    [StructLayout(LayoutKind.Explicit)]
    public struct StructConverter
    {
        [FieldOffset(0)]
        public short Short;
        [FieldOffset(0)]
        public ushort UShort;
        [FieldOffset(0)]
        public int Int;
        [FieldOffset(0)]
        public uint UInt;
        [FieldOffset(0)]
        public long Long;
        [FieldOffset(0)]
        public ulong ULong;
    }
}
