using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.BigFile
{
    public interface IBigFileStruct
    {
        int StructSize { get; }
        void DebugLog(ILogProxy log);
        int ToBytes(byte[] byteBuffer);
    }
}
