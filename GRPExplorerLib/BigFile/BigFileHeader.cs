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
    public class BigFileHeader
    {
        ILogProxy log = LogManager.GetLogProxy("BigFileHeader");
        IOBuffers buffers = new IOBuffers();

        FileInfo fileInfo;

        internal BigFileHeader(FileInfo _fileInfo)
        {
            fileInfo = _fileInfo;
        }

        public BigFileHeaderStruct ReadHeader(ref BigFileSegmentHeader segmentHeader)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new IOException("Can't read header!");

            log.Info("Reading header from file: " + fileInfo.FullName);

            BigFileHeaderStruct header;
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                header = ReadHeader(fs, ref segmentHeader);
            }

            return header;
        }

        public BigFileHeaderStruct ReadHeader(Stream stream, ref BigFileSegmentHeader segmentHeader)
        {
            BigFileHeaderStruct header = new BigFileHeaderStruct();

            log.Info("Reading big file header...");
            log.Debug("Header struct size: " + header.StructSize);

            byte[] buffer = buffers[header.StructSize];
            stream.Seek(segmentHeader.InfoOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, header.StructSize);

            header = MarshalUtil.BytesToStruct<BigFileHeaderStruct>(buffer);

            log.Info("Header read!");

            return header;
        }

        public void WriteHeader(ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new IOException("Can't read header!");

            log.Info("Writing header to file: " + fileInfo.FullName);

            using (FileStream fs = File.OpenWrite(fileInfo.FullName))
            {
                fs.Seek(segmentHeader.InfoOffset, SeekOrigin.Begin);
                WriteHeader(fs, ref header);
            }
        }

        public void WriteHeader(Stream stream, ref BigFileHeaderStruct header)
        {
            log.Info("Writing header...");

            byte[] buffer = buffers[header.StructSize];
            int headerSize = MarshalUtil.StructToBytes<BigFileHeaderStruct>(header, buffer);
            stream.Write(buffer, 0, headerSize);

            log.Info("Header written!");
        }
    }
}
