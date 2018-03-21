using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.BigFile
{
    [Flags]
    public enum BigFileFlags
    {
        None = 0x0,
        UseThreading = 0x1,
        Compress = 0x2,
        Decompress = 0x4,

    }
}
