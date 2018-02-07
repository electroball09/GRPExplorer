using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GRPExplorerLib.Util;

namespace GRPExplorerLib.BigFile
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BigFileHeader
    {
        public int StructSize { get { return Marshal.SizeOf(this); } }

        internal void DebugLog(LogProxy log)
        {
            log.Debug("> BigFileHeader Dump: ");
            log.Debug("    Signature: " + Encoding.Default.GetString(Signature));
            log.Debug("    Unknown_01: " + Unknown_01);
            log.Debug("    Unknown_02: " + Unknown_02);
            log.Debug("    Unknown_03: " + Unknown_03);
            log.Debug("    InfoOffset: " + InfoOffset);
            log.Debug("    Unknown_04: " + Unknown_04);
            log.Debug("    Unknown_05: " + Unknown_05);
            log.Debug("    Unknown_06: " + Unknown_06);
            log.Debug("    Unknown_07: " + Unknown_07);
            log.Debug("    Unknown_08: " + Unknown_08);
            log.Debug("    Unknown_09: " + Unknown_09);
            log.Debug("    Unknown_10: " + Unknown_10);
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Signature;

        public int Unknown_01;
        public int Unknown_02;
        public int Unknown_03;
        
        public int InfoOffset;
        
        public int Unknown_04;
        public int Unknown_05;
        public int Unknown_06;
        public int Unknown_07;
        public int Unknown_08;
        public int Unknown_09;
        public int Unknown_10;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct BigFileFileCountInfo
    {
        public int StructSize { get { return Marshal.SizeOf(this); } }

        internal void DebugLog(LogProxy log)
        {
            log.Debug("> BigFileFileCountInfo Dump:");
            log.Debug("    Unknown_01: " + Unknown_01);
            log.Debug("       Folders: " + Folders);
            log.Debug("         Files: " + Files);
            log.Debug("    Unknown_02: " + Encoding.Default.GetString(Unknown_02));
        }

        public short Unknown_01;
        public short Folders;
        public int Files;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
        public byte[] Unknown_02;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BigFileFileInfo
    {
        public int StructSize { get { return Marshal.SizeOf(this); } }

        internal void DebugLog(LogProxy log)
        {
            log.Debug("> BigFileFileInfo Dump:");
            log.Debug("        Offset: " +     Offset);
            log.Debug("           Key: " +        Key);
            log.Debug("    Unknown_01: " + Unknown_01);
            log.Debug("    Unknown_02: " + Unknown_02);
            log.Debug("        Folder: " +     Folder);
            log.Debug("     TimeStamp: " +  TimeStamp.UnixTime());
            log.Debug("         Flags: " +      Flags);
            log.Debug("    FileNumber: " + FileNumber);
            log.Debug("         CRC32: " + Encoding.Default.GetString(CRC32));
            log.Debug("          Name: " + Encoding.Default.GetString(Name));
            log.Debug("    Unknown_03: " + Unknown_03);
            log.Debug("           ZIP: " +        ZIP);
        }

        public int Offset;
        public int Key;
        public int Unknown_01;
        public short Unknown_02;
        public short Folder;
        public int TimeStamp;
        public int Flags;
        public int FileNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] CRC32;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public byte[] Name;
        public int Unknown_03;
        public int ZIP;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BigFileFolderInfo
    {
        public int StructSize { get { return Marshal.SizeOf(this); } }

        internal void DebugLog(LogProxy log)
        {
            log.Debug("> BigFileFolderInfo Dump:");
            log.Debug("        Unknown_01: " + Unknown_01);
            log.Debug("    PreviousFolder: " + PreviousFolder);
            log.Debug("        NextFolder: " + NextFolder);
            log.Debug("        Unknown_02: " + Unknown_02);
            log.Debug("              Name: " + Encoding.Default.GetString(Name));
        }

        public int Unknown_01;
        public short PreviousFolder;
        public short NextFolder;
        public short Unknown_02;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 54)]
        public byte[] Name;
    }
}
