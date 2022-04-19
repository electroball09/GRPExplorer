using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.Logging
{
    [Flags]
    public enum LogFlags : uint
    {
        None = 0x0,
        All = 0xffffffff,
        Debug = 0x1,
        Info = 0x2,
        Error = 0x4
    }
}
