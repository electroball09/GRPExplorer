using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using GRPExplorerLib.Util;
using System.Diagnostics;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public class BigFileSegment
    {
        ILogProxy log = LogManager.GetLogProxy("BigFileSegment");
        IOBuffers buffers = new IOBuffers();

        FileInfo fileInfo;

        internal BigFileSegment(FileInfo _fileInfo)
        {
            fileInfo = _fileInfo;
        }

        public BigFileSegmentHeader ReadSegmentHeader()
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new IOException("Can't read segment header!");

            log.Info("Reading segment header from file: " + fileInfo.FullName);

            BigFileSegmentHeader header;
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                header = ReadSegmentHeader(fs);
            }

            return header;
        }

        public BigFileSegmentHeader ReadSegmentHeader(Stream stream)
        {
            BigFileSegmentHeader header = new BigFileSegmentHeader();

            log.Info("Reading segment header...");
            log.Debug("Header struct size: " + header.StructSize);

            byte[] buffer = buffers[header.StructSize];
            stream.Read(buffer, 0, header.StructSize);

            header = MarshalUtil.BytesToStruct<BigFileSegmentHeader>(buffer);

            header.DebugLog(log);

            log.Info("Segment header read!");

            return header;
        }

        public void WriteSegmentHeader(ref BigFileSegmentHeader header)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new IOException("Can't write segment header!");

            log.Info("Writing segment header to file: " + fileInfo.FullName);

            using (FileStream fs = File.OpenWrite(fileInfo.FullName))
            {
                WriteSegmentHeader(fs, ref header);
            }
        }

        public void WriteSegmentHeader(Stream stream, ref BigFileSegmentHeader header)
        {
            log.Info("Writing segment header...");

            byte[] buffer = buffers[header.StructSize];
            int headerSize = MarshalUtil.StructToBytes<BigFileSegmentHeader>(header, buffer);

            stream.Write(buffer, 0, headerSize);

            int blankBytesToWrite = (header.InfoOffset - header.StructSize);

            log.Debug("Blank bytes: " + blankBytesToWrite.ToString());

            for (int i = 0; i < blankBytesToWrite; i++)
                stream.WriteByte(0x00);

            log.Info("Segment header written!");
        }
    }
}
