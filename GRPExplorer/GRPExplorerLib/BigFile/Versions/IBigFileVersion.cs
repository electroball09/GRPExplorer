using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.BigFile.Versions
{
    public interface IBigFileVersion
    {
        short Identifier { get; }
        string VersionName { get; }

        IBigFileFileInfo CreateFileInfo();
        IBigFileFolderInfo CreateFolderInfo();
    }
}
