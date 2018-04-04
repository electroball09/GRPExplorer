using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.BigFile.Versions
{
    public interface IBigFileVersion
    {
        short Identifier { get; }

        IBigFileFileInfo CreateFileInfo();
        IBigFileFolderInfo CreateFolderInfo();
    }
}
