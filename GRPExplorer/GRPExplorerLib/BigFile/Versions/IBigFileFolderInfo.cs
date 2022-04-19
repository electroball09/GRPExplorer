using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.BigFile.Versions
{
    public interface IBigFileFolderInfo : IBigFileStruct
    {
            short Unknown_01 { get; set; }
            short Unknown_02 { get; set; }
            short PreviousFolder { get; set; }
            short NextFolder { get; set; }
            short Unknown_03 { get; set; }
            byte[] Name { get; set; }

        IBigFileFolderInfo FromBytes(byte[] byteBuffer);
    }
}
