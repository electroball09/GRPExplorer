using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.BigFile
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BigFileSegmentHeader //: IBigFileStruct 
    {
        public int StructSize { get { return Marshal.SizeOf(this); } }

        public void DebugLog(ILogProxy log)
        {
            log.Debug("> BigFileSegmentHeader Dump: ");
            log.Debug("    Signature: " + Encoding.Default.GetString(Signature));
            log.Debug("    Unknown_01: " + Unknown_01);
            log.Debug("    Unknown_02: " + Unknown_02);
            log.Debug("    Num_Segments: " + Num_Segments);
            log.Debug("    Segment: " + Segment);
            log.Debug("    Unknown_03: " + Unknown_03);
            log.Debug("    InfoOffset: " + InfoOffset);
            //log.Debug("    Unknown_04: " + Unknown_04);
            log.Debug("    Unknown_05: " + Unknown_05);
            log.Debug("    Unknown_06: " + Unknown_06);
            log.Debug("    Unknown_07: " + Unknown_07);
            log.Debug("    Unknown_08: " + Unknown_08);
            log.Debug("    Unknown_09: " + Unknown_09);
            log.Debug("    Unknown_10: " + Unknown_10);
            log.Debug("    Unknown_11: " + Unknown_11);
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Signature;

        public byte Segment;
        public byte Num_Segments;

        public short Unknown_01;

        //public int Unknown_01;
        public int Unknown_02;

        public int Unknown_03;
        //public int Unknown_04;

        public int InfoOffset;

        public int Unknown_05;
        public int Unknown_06;
        public int Unknown_07;
        public int Unknown_08;
        public int Unknown_09;
        public int Unknown_10;
        public int Unknown_11;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct BigFileHeaderStruct
    {
        public int StructSize { get { return Marshal.SizeOf(this); } }

        internal void DebugLog(ILogProxy log)
        {
            log.Debug("> BigFileHeader Dump:");
            log.Debug("    BigFileVersion: " + BigFileVersion);
            log.Debug("       Folders: " + Folders);
            log.Debug("         Files: " + Files);
            log.Debug("    Unknown_02: " + Encoding.Default.GetString(Unknown_02));
        }

        public short BigFileVersion;
        public short Folders;
        public int Files;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
        public byte[] Unknown_02;
    }
}
