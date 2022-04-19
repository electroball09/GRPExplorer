using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;
using GRPExplorerLib.YetiObjects;

namespace GRPExplorerLib.BigFile.Versions
{
    public class BigFileVersion_GRP : IBigFileVersion
    {
        public short Identifier
        {
            get
            {
                return 0x86;
            }
        }

        public string VersionName
        {
            get
            {
                return "GRP";
            }
        }

        public IBigFileFileInfo CreateFileInfo()
        {
            IBigFileFileInfo newStruct = new BigFileFileInfo_GRP();
            newStruct.CRC32 = new byte[4];
            newStruct.Name = new byte[60];
            return newStruct;
        }

        public IBigFileFolderInfo CreateFolderInfo()
        {
            IBigFileFolderInfo newStruct = new BigFileFolderInfo_GRP();
            newStruct.Name = new byte[54];
            return newStruct;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BigFileFileInfo_GRP : IBigFileFileInfo
    {
        int offset;
        int key;
        int unknown_01;
        short fileType;
        short folder;
        int timeStamp;
        int flags;
        int fileNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        byte[] crc32;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        byte[] name;
        int unknown_03;
        int zip;

        public int Offset { get { return offset; } set { offset = value; } }
        public int Key { get { return key; } set { key = value; } }
        public int Unknown_01 { get { return unknown_01; } set { unknown_01 = value; } }
        public YetiObjectType FileType { get { return (YetiObjectType)fileType; } set { fileType = (short)value; } }
        public short Folder { get { return folder; } set { folder = value; } }
        public int TimeStamp { get { return timeStamp; } set { timeStamp = value; } }
        public int Flags { get { return flags; } set { flags = value; } }
        public int FileNumber { get { return fileNumber; } set { fileNumber = value; } }
        public byte[] CRC32 { get { return crc32; } set { crc32 = value; } }
        public byte[] Name { get { return name; } set { name = value; } }
        public int Unknown_03 { get { return unknown_03; } set { unknown_03 = value; } }
        public int ZIP { get { return zip; } set { zip = value; } }

        public int StructSize { get { return Marshal.SizeOf(this); } }

        public void DebugLog(ILogProxy log)
        {
            log.Debug(string.Format("> BigFileFileInfo Dump:"));
            log.Debug(string.Format("        Offset: {0:X8}", Offset));
            log.Debug(string.Format("           Key: {0:X8}", Key));
            log.Debug(string.Format("    Unknown_01: {0:X8}", Unknown_01));
            log.Debug(string.Format("      FileType: {0}", FileType));
            log.Debug(string.Format("        Folder: {0:X4}", Folder));
            log.Debug(string.Format("     TimeStamp: {0}", TimeStamp.UnixTime()));
            log.Debug(string.Format("         Flags: {0}", Convert.ToString(Flags, 2)));
            log.Debug(string.Format("    FileNumber: {0}", FileNumber));
            log.Debug(string.Format("         CRC32: {0}", Encoding.Default.GetString(CRC32)));
            log.Debug(string.Format("          Name: {0}", Encoding.Default.GetString(Name)));
            log.Debug(string.Format("    Unknown_03: {0:X8}", Unknown_03));
            log.Debug(string.Format("           ZIP: {0}", ZIP));
        }

        public int ToBytes(byte[] byteBuffer)
        {
            return MarshalUtil.StructToBytes(this, byteBuffer);
        }

        public IBigFileFileInfo FromBytes(byte[] bytes)
        {
            return MarshalUtil.BytesToStruct<BigFileFileInfo_GRP>(bytes);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BigFileFolderInfo_GRP : IBigFileFolderInfo
    {
        short unknown_01;
        short unknown_02;
        short previousFolder;
        short nextFolder;
        short unknown_03;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 54)]
        byte[] name;

        public short Unknown_01 { get { return unknown_01; } set { unknown_01 = value; } }
        public short Unknown_02 { get { return unknown_02; } set { unknown_02 = value; } }
        public short PreviousFolder { get { return previousFolder; } set { previousFolder = value; } }
        public short NextFolder { get { return nextFolder; } set { nextFolder = value; } }
        public short Unknown_03 { get { return unknown_03; } set { unknown_03 = value; } }
        public byte[] Name { get { return name; } set { name = value; } }

        public int StructSize { get { return Marshal.SizeOf(this); } }

        public void DebugLog(ILogProxy log)
        {
            log.Debug("> BigFileFolderInfo Dump:");
            log.Debug("        Unknown_01: " + Unknown_01);
            log.Debug("    PreviousFolder: " + PreviousFolder);
            log.Debug("        NextFolder: " + NextFolder);
            log.Debug("        Unknown_02: " + Unknown_02);
            log.Debug("              Name: " + Encoding.Default.GetString(Name));
        }

        public int ToBytes(byte[] byteBuffer)
        {
            return MarshalUtil.StructToBytes(this, byteBuffer);
        }

        public IBigFileFolderInfo FromBytes(byte[] bytes)
        {
            return MarshalUtil.BytesToStruct<BigFileFolderInfo_GRP>(bytes);
        }
    }
}
