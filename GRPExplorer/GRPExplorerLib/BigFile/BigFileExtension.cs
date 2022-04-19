using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public static class BigFileExtension
    {
        public static void Copy(this IBigFileFileInfo source, IBigFileFileInfo target)
        {
            target.Offset = source.Offset;
            target.Key = source.Key;
            target.Unknown_01 = source.Unknown_01;
            target.FileType = source.FileType;
            target.Folder = source.Folder;
            target.TimeStamp = source.TimeStamp;
            target.Flags = source.Flags;
            target.FileNumber = source.FileNumber;
            Array.Copy(source.CRC32, target.CRC32, source.CRC32.Length);
            Array.Copy(source.Name, target.Name, source.Name.Length);
            target.Unknown_03 = source.Unknown_03;
            target.ZIP = source.ZIP;
        }

        public static void Copy(this IBigFileFolderInfo source, IBigFileFolderInfo target)
        {
            target.Unknown_01 = source.Unknown_01;
            target.PreviousFolder = source.PreviousFolder;
            target.NextFolder = source.NextFolder;
            target.Unknown_02 = source.Unknown_02;
            Array.Copy(source.Name, target.Name, source.Name.Length);
        }
    }
}
