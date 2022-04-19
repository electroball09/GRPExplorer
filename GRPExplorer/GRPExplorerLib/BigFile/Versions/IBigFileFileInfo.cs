using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.YetiObjects;

namespace GRPExplorerLib.BigFile.Versions
{
    public interface IBigFileFileInfo : IBigFileStruct
    {
              int Offset { get; set; }
                 int Key { get; set; }
          int Unknown_01 { get; set; } // THIS IS ALWAYS 0
   YetiObjectType FileType { get; set; }
            short Folder { get; set; }
           int TimeStamp { get; set; }
               int Flags { get; set; }
          int FileNumber { get; set; }
            byte[] CRC32 { get; set; }
             byte[] Name { get; set; }
          int Unknown_03 { get; set; } // THIS IS ALWAYS 1
                 int ZIP { get; set; }

        IBigFileFileInfo FromBytes(byte[] byteBuffer);
    }
}
